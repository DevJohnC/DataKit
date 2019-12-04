using DataKit.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataKit.Mapping.Binding
{
	public abstract class DataModelBindingBuilder<TSourceModel, TSourceField, TTargetModel, TTargetField>
		where TSourceModel : IDataModel<TSourceModel, TSourceField>
		where TTargetModel : IDataModel<TTargetModel, TTargetField>
		where TSourceField : IModelField<TSourceModel, TSourceField>
		where TTargetField : IModelField<TTargetModel, TTargetField>
	{
		public TSourceModel SourceModel { get; }
		public TTargetModel TargetModel { get; }

		private readonly IBindingPairProvider<TSourceModel, TSourceField, TTargetModel, TTargetField> _defaultBindigPairProvider;

		private readonly IBindingRule<TSourceModel, TSourceField, TTargetModel, TTargetField>[] _autoBindingRules =
			new IBindingRule<TSourceModel, TSourceField, TTargetModel, TTargetField>[]
			{
				CopyIdenticalValueTypesRule<TSourceModel, TSourceField, TTargetModel, TTargetField>.Instance,
				ConvertWithExplicitCastRule<TSourceModel, TSourceField, TTargetModel, TTargetField>.Instance,
				ConvertNumericTypesRule<TSourceModel, TSourceField, TTargetModel, TTargetField>.Instance,
				ConvertWithTryParseRule<TSourceModel, TSourceField, TTargetModel, TTargetField>.Instance,
				ConvertWithToStringRule<TSourceModel, TSourceField, TTargetModel, TTargetField>.Instance,
			};

		protected readonly List<ModelFieldBinding<TSourceField, TTargetField>> FieldBindings
			= new List<ModelFieldBinding<TSourceField, TTargetField>>();

		public DataModelBindingBuilder(TSourceModel sourceModel, TTargetModel targetModel) :
			this(sourceModel, targetModel, FlattenNameMatchBindingPairProvider<TSourceModel, TSourceField, TTargetModel, TTargetField>.Instance)
		{
		}

		public DataModelBindingBuilder(
			TSourceModel sourceModel, TTargetModel targetModel,
			IBindingPairProvider<TSourceModel, TSourceField, TTargetModel, TTargetField> defaultBindingPairProvider
			)
		{
			SourceModel = sourceModel;
			TargetModel = targetModel;
			_defaultBindigPairProvider = defaultBindingPairProvider;
		}

		public virtual bool IsBound(ModelFieldBindingSource<TSourceField> bindingSource)
		{
			return FieldBindings.Any(q => q.BindingSource.Path.SequenceEqual(bindingSource.Path));
		}

		public virtual bool IsBound(ModelFieldBindingTarget<TTargetField> bindingTarget)
		{
			return FieldBindings.Any(q => q.BindingTarget.Path.SequenceEqual(bindingTarget.Path));
		}

		/// <summary>
		/// Auto-bind fields using the default binding pair provider on the builder.
		/// </summary>
		public virtual DataModelBindingBuilder<TSourceModel, TSourceField, TTargetModel, TTargetField> AutoBind(IEnumerable<DataModelBinding<TSourceField, TTargetField>> bindings = null)
			=> AutoBind(_defaultBindigPairProvider, bindings);

		/// <summary>
		/// Auto-bind fields using the provided binding pair provider.
		/// </summary>
		/// <param name="bindingPairProvider"></param>
		public virtual DataModelBindingBuilder<TSourceModel, TSourceField, TTargetModel, TTargetField> AutoBind(IBindingPairProvider<TSourceModel, TSourceField, TTargetModel, TTargetField> bindingPairProvider,
			IEnumerable<DataModelBinding<TSourceField, TTargetField>> bindings = null)
		{
			var candidateBindingPairs = bindingPairProvider.GetBindingPairs(
				SourceModel, TargetModel
				).ToArray();

			var autoBindingRules = _autoBindingRules;
			if (bindings != null)
			{
				autoBindingRules = _autoBindingRules
					.Concat(new[] { new CopyBindingsRule<TSourceModel, TSourceField, TTargetModel, TTargetField>(bindings) })
					.ToArray();
			}

			foreach (var bindingPair in candidateBindingPairs)
			{
				if (!bindingPair.Source.Field.CanRead || !bindingPair.Target.Field.CanWrite)
					continue;

				foreach (var bindingRule in autoBindingRules)
				{
					bindingRule.AddBindings(this, bindingPair);
				}
			}

			return this;
		}

		public virtual DataModelBindingBuilder<TSourceModel, TSourceField, TTargetModel, TTargetField> Bind(ModelFieldBindingTarget<TTargetField> bindingTarget,
			ModelFieldBindingSource<TSourceField> bindingSource,
			DataModelBinding<TSourceField, TTargetField> binding)
		{
			var sourcePathBase = bindingSource.Path;
			var targetPathBase = bindingTarget.Path;

			foreach (var inputFieldBinding in binding.FieldBindings)
			{
				var target = new ModelFieldBindingTarget<TTargetField>(
					targetPathBase.Concat(inputFieldBinding.BindingTarget.Path).ToArray(),
					inputFieldBinding.TargetField
					);

				if (IsBound(target))
					continue;

				var source = new ModelFieldBindingSource<TSourceField>(
					sourcePathBase.Concat(inputFieldBinding.BindingSource.Path).ToArray(),
					inputFieldBinding.SourceField
					);

				var fieldBinding = new ModelFieldBinding<TSourceField, TTargetField>(
					source,
					target,
					inputFieldBinding.Transformation
				);

				FieldBindings.Add(fieldBinding);
			}

			return this;
		}

		public virtual DataModelBindingBuilder<TSourceModel, TSourceField, TTargetModel, TTargetField> Bind(ModelFieldBindingTarget<TTargetField> bindingTarget,
			ModelFieldBindingSource<TSourceField> bindingSource,
			IBindingTransformFactory bindingTransformFactory)
		{
			if (bindingSource == null)
				throw new ArgumentNullException(nameof(bindingSource));

			if (bindingTarget == null)
				throw new ArgumentNullException(nameof(bindingTarget));

			var bindingTransformation = bindingTransformFactory.CreateTransformation(
				bindingSource.Field, bindingTarget.Field
				);

			var fieldBinding = new ModelFieldBinding<TSourceField, TTargetField>(
				bindingSource, bindingTarget, bindingTransformation
				);

			FieldBindings.Add(fieldBinding);

			return this;
		}
	}

	public abstract class DataModelBindingBuilder<TBuilder, TSourceModel, TSourceField, TTargetModel, TTargetField> :
		DataModelBindingBuilder<TSourceModel, TSourceField, TTargetModel, TTargetField>
		where TBuilder : DataModelBindingBuilder<TBuilder, TSourceModel, TSourceField, TTargetModel, TTargetField>
		where TSourceModel : IDataModel<TSourceModel, TSourceField>
		where TTargetModel : IDataModel<TTargetModel, TTargetField>
		where TSourceField : IModelField<TSourceModel, TSourceField>
		where TTargetField : IModelField<TTargetModel, TTargetField>
	{
		public DataModelBindingBuilder(TSourceModel sourceModel, TTargetModel targetModel) :
			base(sourceModel, targetModel)
		{
		}

		public DataModelBindingBuilder(TSourceModel sourceModel, TTargetModel targetModel, IBindingPairProvider<TSourceModel, TSourceField, TTargetModel, TTargetField> defaultBindingPairProvider) :
			base(sourceModel, targetModel, defaultBindingPairProvider)
		{
		}

		public new TBuilder AutoBind(IEnumerable<DataModelBinding<TSourceField, TTargetField>> bindings = null)
			=> (TBuilder)base.AutoBind(bindings);

		public new TBuilder AutoBind(IBindingPairProvider<TSourceModel, TSourceField, TTargetModel, TTargetField> bindingPairProvider,
			IEnumerable<DataModelBinding<TSourceField, TTargetField>> bindings = null)
			=> (TBuilder)base.AutoBind(bindingPairProvider, bindings);

		public new TBuilder Bind(ModelFieldBindingTarget<TTargetField> bindingTarget,
			ModelFieldBindingSource<TSourceField> bindingSource,
			DataModelBinding<TSourceField, TTargetField> binding)
			=> (TBuilder)base.Bind(bindingTarget, bindingSource, binding);

		public new TBuilder Bind(ModelFieldBindingTarget<TTargetField> bindingTarget,
			ModelFieldBindingSource<TSourceField> bindingSource,
			IBindingTransformFactory bindingTransformFactory)
			=> (TBuilder)base.Bind(bindingTarget, bindingSource, bindingTransformFactory);
	}

	/// <summary>
	/// Base class for building data model builders from.
	/// </summary>
	/// <remarks>
	/// It is intended that developers sub-class this builder type for their specific usage scenarios and to make the
	/// generic type parameters involved contracted by your type.
	/// 
	/// For an example see <see cref="TypeBindingBuilder"/>.
	/// </remarks>
	/// <typeparam name="TSourceModel"></typeparam>
	/// <typeparam name="TTargetModel"></typeparam>
	public abstract class DataModelBindingBuilder<TBuilder, TBinding, TSourceModel, TSourceField, TTargetModel, TTargetField> :
		DataModelBindingBuilder<TBuilder, TSourceModel, TSourceField, TTargetModel, TTargetField>
		where TBuilder : DataModelBindingBuilder<TBuilder, TBinding, TSourceModel, TSourceField, TTargetModel, TTargetField>
		where TBinding : DataModelBinding
		where TSourceModel : IDataModel<TSourceModel, TSourceField>
		where TTargetModel : IDataModel<TTargetModel, TTargetField>
		where TSourceField : IModelField<TSourceModel, TSourceField>
		where TTargetField : IModelField<TTargetModel, TTargetField>
	{
		public DataModelBindingBuilder(TSourceModel sourceModel, TTargetModel targetModel) :
			base(sourceModel, targetModel)
		{
		}

		public DataModelBindingBuilder(TSourceModel sourceModel, TTargetModel targetModel, IBindingPairProvider<TSourceModel, TSourceField, TTargetModel, TTargetField> defaultBindingPairProvider) :
			base(sourceModel, targetModel, defaultBindingPairProvider)
		{
		}

		protected abstract TBinding BindingFactory(TSourceModel sourceModel, TTargetModel targetModel, IReadOnlyList<ModelFieldBinding<TSourceField, TTargetField>> fieldBindings);

		public TBinding BuildBinding()
		{
			return BindingFactory(SourceModel, TargetModel, FieldBindings);
		}
	}
}
