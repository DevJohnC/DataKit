using DataKit.Modelling;

namespace DataKit.Mapping.Binding
{
	public class ModelFieldBindingTarget<TField> : FieldGraphPath<TField>
		where TField : IModelField
	{
		public ModelFieldBindingTarget(string[] path, TField field) :
			base(path, field)
		{
		}
	}
}
