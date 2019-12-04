using DataKit.Modelling;

namespace DataKit.Mapping.Binding
{
	public class CopyIdenticalValueTypesRule<TSourceModel, TSourceField, TTargetModel, TTargetField> : IBindingRule<TSourceModel, TSourceField, TTargetModel, TTargetField>
		where TSourceModel : IDataModel<TSourceModel, TSourceField>
		where TTargetModel : IDataModel<TTargetModel, TTargetField>
		where TSourceField : IModelField<TSourceModel, TSourceField>
		where TTargetField : IModelField<TTargetModel, TTargetField>
	{
		public static CopyIdenticalValueTypesRule<TSourceModel, TSourceField, TTargetModel, TTargetField> Instance { get; }
			= new CopyIdenticalValueTypesRule<TSourceModel, TSourceField, TTargetModel, TTargetField>();

		private static bool MatchingValidTypes(DataType sourceType, DataType targetType)
		{
			return IsValidType(sourceType) && IsValidType(targetType) && sourceType.Type == targetType.Type;
		}

		private static bool IsValidType(DataType dataType)
		{
			if (dataType.IsPureEnumerable)
				return false;

			return dataType.Type.IsValueType || dataType.Type == typeof(string);
		}

		public void AddBindings(
			DataModelBindingBuilder<TSourceModel, TSourceField, TTargetModel, TTargetField> builder,
			BindingPair<TSourceField, TTargetField> bindingPair)
		{
			if (!MatchingValidTypes(bindingPair.Source.FieldType, bindingPair.Target.FieldType))
				return;

			if (builder.IsBound(bindingPair.Target))
				return;

			builder.Bind(bindingPair.Target, bindingPair.Source, ValueCopyTransformFactory.Instance);
		}

		public class ValueCopyTransformFactory : IBindingTransformFactory
		{
			public static ValueCopyTransformFactory Instance { get; }
				= new ValueCopyTransformFactory();

			public BindingTransformation CreateTransformation<TSource, TTarget>(
				IModelFieldOf<TSource> sourceField, IModelFieldOf<TTarget> targetField)
				=> Transform;

			private static bool Transform(BindingContext context, object originalValue, out object transformedValue)
			{
				transformedValue = originalValue;
				return true;
			}
		}
	}
}
