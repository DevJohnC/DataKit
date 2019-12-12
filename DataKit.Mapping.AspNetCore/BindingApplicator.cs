using DataKit.Mapping.Binding;
using DataKit.Modelling.TypeModels;

namespace DataKit.Mapping.AspNetCore
{
	public abstract class BindingApplicator
	{
		public abstract DataModelBinding<PropertyField, PropertyField> CreateBinding();
	}

	public class BindingApplicator<TFrom, TTo> : BindingApplicator
		where TFrom : class
		where TTo : class
	{
		private readonly IBindingOverride<TFrom, TTo> _bindingOverride;

		public BindingApplicator(IBindingOverride<TFrom, TTo> bindingOverride)
		{
			_bindingOverride = bindingOverride;
		}

		public override DataModelBinding<PropertyField, PropertyField> CreateBinding()
		{
			var builder = new TypeBindingBuilder<TFrom, TTo>();
			_bindingOverride.BindFields(builder);
			return builder.BuildBinding();
		}
	}
}
