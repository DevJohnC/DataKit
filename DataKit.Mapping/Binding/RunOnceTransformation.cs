namespace DataKit.Mapping.Binding
{
	public class RunOnceTransformation
	{
		public static BindingTransformation Create(BindingTransformation bindingTransformation)
		{
			return new RunOnceTransformationImpl(bindingTransformation).Transform;
		}

		private class RunOnceTransformationImpl
		{
			private readonly BindingTransformation _bindingTransformation;

			public RunOnceTransformationImpl(BindingTransformation bindingTransformation)
			{
				_bindingTransformation = bindingTransformation;
			}

			public bool Transform(BindingContext bindingContext, object input, out object output)
			{
				if (bindingContext.TryGetCachedValue(this, out output))
					return true;

				if (_bindingTransformation(bindingContext, input, out output))
				{
					bindingContext.AddCachedValue(this, output);
					return true;
				}

				return false;
			}
		}
	}
}
