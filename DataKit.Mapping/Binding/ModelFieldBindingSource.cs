using System;
using DataKit.Modelling;

namespace DataKit.Mapping.Binding
{
	public class ModelFieldBindingSource<TField> : FieldGraphPath<TField>
		where TField : IModelField
	{
		public ModelFieldBindingSource(string[] path, TField field) :
			base(path, field)
		{
		}
	}
}
