using DataKit.Modelling;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DataKit.Mapping.Binding
{
	public class ConvertWithExplicitCastRule<TSourceModel, TSourceField, TTargetModel, TTargetField> : IBindingRule<TSourceModel, TSourceField, TTargetModel, TTargetField>
		where TSourceModel : IDataModel<TSourceModel, TSourceField>
		where TTargetModel : IDataModel<TTargetModel, TTargetField>
		where TSourceField : IModelField<TSourceModel, TSourceField>
		where TTargetField : IModelField<TTargetModel, TTargetField>
	{
		public static ConvertWithExplicitCastRule<TSourceModel, TSourceField, TTargetModel, TTargetField> Instance { get; }
			= new ConvertWithExplicitCastRule<TSourceModel, TSourceField, TTargetModel, TTargetField>();

		public void AddBindings(
			DataModelBindingBuilder<TSourceModel, TSourceField, TTargetModel, TTargetField> builder,
			BindingPair<TSourceField, TTargetField> bindingPair)
		{
			if (builder.IsBound(bindingPair.Target))
				return;

			var castMethod = GetExplicitCast(
				bindingPair.Source.FieldType.Type,
				bindingPair.Source.FieldType.Type,
				bindingPair.Target.FieldType.Type
				);
			if (castMethod == null)
				castMethod = GetExplicitCast(
					bindingPair.Target.FieldType.Type,
					bindingPair.Source.FieldType.Type,
					bindingPair.Target.FieldType.Type
					);

			if (castMethod == null)
				return;

			builder.Bind(bindingPair.Target, bindingPair.Source, new TransformFactory(castMethod));
		}

		private MethodInfo GetExplicitCast(Type declaringType, Type fromType, Type toType)
		{
			return declaringType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
				.FirstOrDefault(q => q.Name == "op_Explicit" && q.ReturnType == toType && q.GetParameters()[0].ParameterType == fromType);
		}

		private struct TransformFactory : IBindingTransformFactory
		{
			private readonly MethodInfo _explicitCastMethod;

			public TransformFactory(MethodInfo explicitCastMethod)
			{
				_explicitCastMethod = explicitCastMethod ?? throw new ArgumentNullException(nameof(explicitCastMethod));
			}

			public BindingTransformation CreateTransformation<TSource, TTarget>(IModelFieldOf<TSource> sourceField, IModelFieldOf<TTarget> targetField)
			{
				var tryParseMethod = _explicitCastMethod;

				var fromParameter = Expression.Parameter(typeof(TSource), "from");
				var toParameter = Expression.Parameter(typeof(TTarget).MakeByRefType(), "to");

				var lambda = Expression.Lambda<BindingTransformation>(
					Expression.Block(
						Expression.Assign(toParameter, Expression.Call(_explicitCastMethod, fromParameter)),
						Expression.Constant(true)
						), fromParameter, toParameter
					);
				return lambda.Compile();
			}
		}
	}
}
