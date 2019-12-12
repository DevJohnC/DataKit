using DataKit.Mapping.Binding;

namespace DataKit.Mapping.AspNetCore
{
	public interface IBindingOverride<TFrom, TTo>
		where TFrom : class
		where TTo : class
	{
		void BindFields(TypeBindingBuilder<TFrom, TTo> builder);
	}
}
