using System;
using DataKit.Modelling;

namespace DataKit.Mapping.Binding
{
	public abstract class ModelFieldBinding
	{
		protected ModelFieldBinding(IModelField sourceField, IModelField targetField, BindingTransformation transformation)
		{
			SourceField = sourceField ?? throw new ArgumentNullException(nameof(sourceField));
			TargetField = targetField ?? throw new ArgumentNullException(nameof(targetField));
			Transformation = transformation ?? throw new ArgumentNullException(nameof(transformation));
		}

		public IModelField SourceField { get; }

		public IModelField TargetField { get; }

		public BindingTransformation Transformation { get; }
	}

	public class ModelFieldBinding<TSourceField, TTargetField> : ModelFieldBinding
		where TSourceField : IModelField
		where TTargetField : IModelField
	{
		public ModelFieldBinding(
			ModelFieldBindingSource<TSourceField> bindingSource,
			ModelFieldBindingTarget<TTargetField> bindingTarget,
			BindingTransformation transformation) :
			base(bindingSource.Field, bindingTarget.Field, transformation)
		{
			BindingSource = bindingSource;
			BindingTarget = bindingTarget;
		}

		public new TSourceField SourceField => BindingSource.Field;

		public ModelFieldBindingSource<TSourceField> BindingSource { get; }

		public new TTargetField TargetField => BindingTarget.Field;

		public ModelFieldBindingTarget<TTargetField> BindingTarget { get; }
	}
}
