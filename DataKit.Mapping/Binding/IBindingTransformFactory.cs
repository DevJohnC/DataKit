using DataKit.Modelling;

namespace DataKit.Mapping.Binding
{
	public interface IBindingTransformFactory
	{
		BindingTransformation CreateTransformation<TSource, TTarget>(IModelFieldOf<TSource> sourceField, IModelFieldOf<TTarget> targetField);
	}
}
