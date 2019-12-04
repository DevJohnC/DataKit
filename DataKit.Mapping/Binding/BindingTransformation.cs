namespace DataKit.Mapping.Binding
{
	public delegate bool BindingTransformation(BindingContext context, object originalValue, out object transformedValue);
}
