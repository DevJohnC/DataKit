using DataKit.Modelling;

namespace DataKit.Mapping.Binding
{
	public static class IBindingTransformFactoryExtensions
	{
		public static BindingTransformation CreateTransformation(
			this IBindingTransformFactory bindingTransformFactory,
			IModelField source, IModelField target
			)
		{
			var transformationBuilder = new FieldBindingTransformationBuilder(source, target, bindingTransformFactory);
			return transformationBuilder.CreateBindingTransformation();
		}

		private class FieldBindingTransformationBuilder : ModelVisitor
		{
			private readonly IModelField _source;
			private readonly IModelField _target;
			private readonly IBindingTransformFactory _transformFactory;

			public BindingTransformation Result { get; private set; }

			public FieldBindingTransformationBuilder(
				IModelField source,
				IModelField target,
				IBindingTransformFactory transformFactory
				)
			{
				_source = source;
				_target = target;
				_transformFactory = transformFactory;
			}

			public override IModelNode VisitField<T>(IModelFieldOf<T> field)
			{
				var subVisitor = new BindingTargetVisitor<T>(field, _transformFactory);
				_target.Accept(subVisitor);
				Result = subVisitor.Result;
				return base.VisitField(field);
			}

			public BindingTransformation CreateBindingTransformation()
			{
				_source.Accept(this);
				return Result;
			}
		}

		private class BindingTargetVisitor<TSource> : ModelVisitor
		{
			private readonly IModelFieldOf<TSource> _source;
			private readonly IBindingTransformFactory _transformFactory;

			public BindingTransformation Result { get; private set; }

			public BindingTargetVisitor(
				IModelFieldOf<TSource> source,
				IBindingTransformFactory transformFactory
				)
			{
				_source = source;
				_transformFactory = transformFactory;
			}

			public override IModelNode VisitField<T>(IModelFieldOf<T> field)
			{
				Result = _transformFactory.CreateTransformation(_source, field);
				return base.VisitField(field);
			}
		}
	}
}
