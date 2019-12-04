using DataKit.Modelling;

namespace DataKit.Mapping.Binding
{
	public class ConvertWithToStringRule<TSourceModel, TSourceField, TTargetModel, TTargetField> : IBindingRule<TSourceModel, TSourceField, TTargetModel, TTargetField>
		where TSourceModel : IDataModel<TSourceModel, TSourceField>
		where TTargetModel : IDataModel<TTargetModel, TTargetField>
		where TSourceField : IModelField<TSourceModel, TSourceField>
		where TTargetField : IModelField<TTargetModel, TTargetField>
	{
		public static ConvertWithToStringRule<TSourceModel, TSourceField, TTargetModel, TTargetField> Instance { get; }
			= new ConvertWithToStringRule<TSourceModel, TSourceField, TTargetModel, TTargetField>();

		public void AddBindings(
			DataModelBindingBuilder<TSourceModel, TSourceField, TTargetModel, TTargetField> builder,
			BindingPair<TSourceField, TTargetField> bindingPair)
		{
			if (bindingPair.Target.FieldType.Type != typeof(string) || builder.IsBound(bindingPair.Target))
				return;

			builder.Bind(bindingPair.Target, bindingPair.Source, ConvertWithToStringTransformFactory.Instance);
		}

		private class ConvertWithToStringTransformFactory : IBindingTransformFactory
		{
			public static ConvertWithToStringTransformFactory Instance { get; }
				= new ConvertWithToStringTransformFactory();

			public BindingTransformation CreateTransformation<TSource, TTarget>(
				IModelFieldOf<TSource> sourceField, IModelFieldOf<TTarget> targetField)
				=> Transform;

			private static bool Transform(BindingContext context, object originalValue, out object transformedValue)
			{
				transformedValue = originalValue.ToString();
				return true;
			}
		}
	}
}
