using DataKit.Modelling;
using System;

namespace DataKit.Mapping.Binding
{
	public struct BindingPair<TSourceField, TTargetField>
		where TSourceField : IModelField
		where TTargetField : IModelField
	{
		public BindingPair(ModelFieldBindingSource<TSourceField> source, ModelFieldBindingTarget<TTargetField> target)
		{
			Source = source ?? throw new ArgumentNullException(nameof(source));
			Target = target ?? throw new ArgumentNullException(nameof(target));
		}

		public ModelFieldBindingSource<TSourceField> Source { get; }

		public ModelFieldBindingTarget<TTargetField> Target { get; }
	}
}
