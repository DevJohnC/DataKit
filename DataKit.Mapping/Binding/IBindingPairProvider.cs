using DataKit.Modelling;
using System.Collections.Generic;

namespace DataKit.Mapping.Binding
{
	public interface IBindingPairProvider<TSourceModel, TSourceField, TTargetModel, TTargetField>
		where TSourceField : IModelField<TSourceModel, TSourceField>
		where TTargetField : IModelField<TTargetModel, TTargetField>
		where TSourceModel : IDataModel<TSourceModel, TSourceField>
		where TTargetModel : IDataModel<TTargetModel, TTargetField>
	{
		IEnumerable<BindingPair<TSourceField, TTargetField>> GetBindingPairs(TSourceModel sourceModel, TTargetModel targetModel);
	}
}
