using DataKit.Mapping.Binding;
using DataKit.Modelling;

namespace DataKit.Mapping
{
	public static class DataModelBindingExtensions
	{
		public static Mapping<TSourceModel, TSourceField, TTargetModel, TTargetField> BuildMapping<TSourceModel, TSourceField, TTargetModel, TTargetField>(
			this DataModelBinding<TSourceModel,TSourceField,TTargetModel,TTargetField> binding
			)
			where TSourceModel : IDataModel<TSourceField>
			where TTargetModel : IDataModel<TTargetField>
			where TSourceField : IModelField
			where TTargetField : IModelField
		{
			return new MappingBuilder<TSourceModel, TSourceField, TTargetModel, TTargetField>(binding)
				.Build();
		}
	}
}
