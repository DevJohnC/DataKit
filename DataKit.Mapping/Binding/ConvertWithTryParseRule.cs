using DataKit.Modelling;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DataKit.Mapping.Binding
{
	public class ConvertWithTryParseRule<TSourceModel, TSourceField, TTargetModel, TTargetField> : IBindingRule<TSourceModel, TSourceField, TTargetModel, TTargetField>
		where TSourceModel : IDataModel<TSourceModel, TSourceField>
		where TTargetModel : IDataModel<TTargetModel, TTargetField>
		where TSourceField : IModelField<TSourceModel, TSourceField>
		where TTargetField : IModelField<TTargetModel, TTargetField>
	{
		public static ConvertWithTryParseRule<TSourceModel, TSourceField, TTargetModel, TTargetField> Instance { get; }
			= new ConvertWithTryParseRule<TSourceModel, TSourceField, TTargetModel, TTargetField>();

		public void AddBindings(
			DataModelBindingBuilder<TSourceModel, TSourceField, TTargetModel, TTargetField> builder,
			BindingPair<TSourceField, TTargetField> bindingPair)
		{
			if (bindingPair.Source.FieldType.Type != typeof(string) || builder.IsBound(bindingPair.Target))
				return;

			var tryParseMethod = GetTryParseMethod(bindingPair.Source.FieldType.Type, bindingPair.Target.FieldType.Type);
			if (tryParseMethod == null)
				return;

			builder.Bind(bindingPair.Target, bindingPair.Source, new TransformFactory(tryParseMethod));
		}

		private MethodInfo GetTryParseMethod(Type sourceType, Type toType)
		{
			if (toType.IsEnum)
				return typeof(Enum).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
					.First(q => q.Name == nameof(Enum.TryParse) && q.IsStatic && q.GetParameters().Length == 3 && q.IsGenericMethodDefinition)
					.MakeGenericMethod(toType);
			return toType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(
				q => q.Name == "TryParse" && q.IsStatic && q.GetParameters().Length == 2
			);
		}

		private struct TransformFactory : IBindingTransformFactory
		{
			private readonly MethodInfo _tryParseMethod;

			public TransformFactory(MethodInfo tryParseMethod)
			{
				_tryParseMethod = tryParseMethod ?? throw new ArgumentNullException(nameof(tryParseMethod));
			}

			public BindingTransformation CreateTransformation<TSource, TTarget>(IModelFieldOf<TSource> sourceField, IModelFieldOf<TTarget> targetField)
			{
				var tryParseMethod = _tryParseMethod;

				var fromParameter = Expression.Parameter(typeof(TSource), "from");
				var toParameter = Expression.Parameter(typeof(TTarget).MakeByRefType(), "to");

				Expression body;
				if (tryParseMethod.DeclaringType == typeof(Enum))
					body = Expression.Call(tryParseMethod, fromParameter, Expression.Constant(false), toParameter);
				else
					body = Expression.Call(tryParseMethod, fromParameter, toParameter);

				return Expression.Lambda<BindingTransformation>(body, fromParameter, toParameter).Compile();
			}
		}
	}
}
