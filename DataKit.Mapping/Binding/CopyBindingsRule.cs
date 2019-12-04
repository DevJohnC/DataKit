using System.Collections.Generic;
using System.Linq;
using DataKit.Modelling;

namespace DataKit.Mapping.Binding
{
	public class CopyBindingsRule<TSourceModel, TSourceField, TTargetModel, TTargetField> : IBindingRule<TSourceModel, TSourceField, TTargetModel, TTargetField>
		where TSourceModel : IDataModel<TSourceModel, TSourceField>
		where TTargetModel : IDataModel<TTargetModel, TTargetField>
		where TSourceField : IModelField<TSourceModel, TSourceField>
		where TTargetField : IModelField<TTargetModel, TTargetField>
	{
		private readonly DataModelBinding<TSourceField, TTargetField>[] _bindings;

		public CopyBindingsRule(IEnumerable<DataModelBinding<TSourceField, TTargetField>> bindings)
		{
			_bindings = bindings.ToArray();
		}

		public void AddBindings(
			DataModelBindingBuilder<TSourceModel, TSourceField, TTargetModel, TTargetField> builder,
			BindingPair<TSourceField, TTargetField> bindingPair
			)
		{
			var binding = _bindings.FirstOrDefault(
				q => q.SourceModel.Equals(bindingPair.Source.Field.FieldModel) &&
					q.TargetModel.Equals(bindingPair.Target.Field.FieldModel)
				);
			if (binding == null)
				return;

			builder.Bind(bindingPair.Target, bindingPair.Source, binding);
		}
	}
}
