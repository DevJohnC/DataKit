using DataKit.Modelling;

namespace DataKit.Mapping.Binding
{
	public class CompositeBindingTransformFactory : IBindingTransformFactory
	{
		private readonly BindingTransformation[] _bindingTransformations;

		public CompositeBindingTransformFactory(params BindingTransformation[] bindingTransformations)
		{
			_bindingTransformations = bindingTransformations;
		}

		public BindingTransformation CreateTransformation<TSource, TTarget>(
			IModelFieldOf<TSource> sourceField,
			IModelFieldOf<TTarget> targetField)
		{
			return Transform;
		}

		private bool Transform(BindingContext context, object input, out object output)
		{
			object swap;
			foreach (var transform in _bindingTransformations)
			{
				if (!transform(context, input, out swap))
				{
					output = default;
					return false;
				}

				input = swap;
			}

			output = input;
			return true;
		}
	}
}
