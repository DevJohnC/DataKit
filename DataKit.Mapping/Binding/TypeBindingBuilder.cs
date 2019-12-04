using DataKit.Modelling;
using DataKit.Modelling.TypeModels;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DataKit.Mapping.Binding
{
	/// <summary>
	/// Builds bindings between <see cref="TypeModel"/> instances.
	/// </summary>
	public class TypeBindingBuilder : DataModelBindingBuilder<TypeBindingBuilder, DataModelBinding<TypeModel, PropertyField, TypeModel, PropertyField>, TypeModel, PropertyField, TypeModel, PropertyField>
	{
		public TypeBindingBuilder(Type sourceType, Type targetType) :
			this(TypeModel.GetModelOf(sourceType), TypeModel.GetModelOf(targetType))
		{
		}

		public TypeBindingBuilder(TypeModel sourceModel, TypeModel targetModel) :
			base(sourceModel, targetModel)
		{
		}

		public TypeBindingBuilder(Type sourceType, Type targetType,
			IBindingPairProvider<TypeModel, PropertyField, TypeModel, PropertyField> defaultBindingPairProvider) :
			this(TypeModel.GetModelOf(sourceType), TypeModel.GetModelOf(targetType), defaultBindingPairProvider)
		{
		}

		public TypeBindingBuilder(TypeModel sourceModel, TypeModel targetModel,
			IBindingPairProvider<TypeModel, PropertyField, TypeModel, PropertyField> defaultBindingPairProvider) :
			base(sourceModel, targetModel, defaultBindingPairProvider)
		{
		}

		protected override DataModelBinding<TypeModel, PropertyField, TypeModel, PropertyField> BindingFactory(TypeModel sourceModel, TypeModel targetModel, IReadOnlyList<ModelFieldBinding<PropertyField, PropertyField>> fieldBindings)
		{
			return new DataModelBinding<TypeModel, PropertyField, TypeModel, PropertyField>(
				sourceModel, targetModel, fieldBindings
				);
		}
	}

	/// <summary>
	/// Builds bindings between <see cref="TypeModel"/> instances.
	/// </summary>
	public class TypeBindingBuilder<TSource, TTarget> :
		DataModelBindingBuilder<TypeBindingBuilder<TSource, TTarget>, DataModelBinding<TypeModel<TSource>, PropertyField, TypeModel<TTarget>, PropertyField>, TypeModel, PropertyField, TypeModel, PropertyField>
	{
		public delegate bool TryConvertDelegate<TFrom, TTo>(TFrom sourceValue, out TTo convertedValue);

		public TypeBindingBuilder() :
			this(TypeModel.GetModelOf<TSource>(), TypeModel.GetModelOf<TTarget>())
		{
		}

		public TypeBindingBuilder(TypeModel<TSource> sourceModel, TypeModel<TTarget> targetModel) :
			base(sourceModel, targetModel)
		{
		}

		public TypeBindingBuilder(IBindingPairProvider<TypeModel, PropertyField, TypeModel, PropertyField> defaultBindingPairProvider) :
			this(TypeModel.GetModelOf<TSource>(), TypeModel.GetModelOf<TTarget>(), defaultBindingPairProvider)
		{
		}

		public TypeBindingBuilder(TypeModel<TSource> sourceModel, TypeModel<TTarget> targetModel,
			IBindingPairProvider<TypeModel, PropertyField, TypeModel, PropertyField> defaultBindingPairProvider) :
			base(sourceModel, targetModel, defaultBindingPairProvider)
		{
		}

		public TypeBindingBuilder<TSource, TTarget> Bind<TFrom, TTo>(Expression<Func<TTarget, TTo>> target,
			Expression<Func<TSource, TFrom>> source,
			DataModelBinding<TypeModel<TFrom>, PropertyField, TypeModel<TTo>, PropertyField> binding)
		{
			return Bind(
				ConvertExpressionToBindingTarget(target),
				ConvertExpressionToBindingSource(source),
				binding
				);
		}

		public TypeBindingBuilder<TSource, TTarget> Bind<T>(Expression<Func<TTarget, T>> target, Expression<Func<TSource, T>> source)
		{
			return Bind(
				ConvertExpressionToBindingTarget(target),
				ConvertExpressionToBindingSource(source),
				CopyIdenticalValueTypesRule<TypeModel, PropertyField, TypeModel, PropertyField>.ValueCopyTransformFactory.Instance
				);
		}

		public TypeBindingBuilder<TSource, TTarget> Bind<TFrom, TTo>(
			Expression<Func<TTarget, TTo>> target,
			Expression<Func<TSource, TFrom>> source,
			TryConvertDelegate<TFrom, TTo> tryConvertDelegate)
		{
			return Bind(
				ConvertExpressionToBindingTarget(target),
				ConvertExpressionToBindingSource(source),
				new TryConvertDelegateFactory<TFrom, TTo>(tryConvertDelegate)
				);
		}

		public TypeBindingBuilder<TSource, TTarget> Bind<TFrom, TTo>(
			Expression<Func<TTarget, TTo>> target,
			Expression<Func<TSource, TFrom>> source,
			Func<TFrom, TTo> alwaysConvertsDelegate)
		{
			return Bind(
				ConvertExpressionToBindingTarget(target),
				ConvertExpressionToBindingSource(source),
				new TryConvertDelegateFactory<TFrom, TTo>((TFrom @in, out TTo @out) =>
				{
					@out = alwaysConvertsDelegate(@in);
					return true;
				}));
		}

		private ModelFieldBindingSource<PropertyField> ConvertExpressionToBindingSource<T>(Expression<Func<TSource, T>> expression)
		{
			var fieldGraphPath = ((TypeModel<TSource>)SourceModel).GetField(expression);
			return new ModelFieldBindingSource<PropertyField>(
				fieldGraphPath.Path, fieldGraphPath.Field
				);
		}

		private ModelFieldBindingTarget<PropertyField> ConvertExpressionToBindingTarget<T>(Expression<Func<TTarget, T>> expression)
		{
			var fieldGraphPath = ((TypeModel<TTarget>)TargetModel).GetField(expression);
			return new ModelFieldBindingTarget<PropertyField>(
				fieldGraphPath.Path, fieldGraphPath.Field
				);
		}

		protected override DataModelBinding<TypeModel<TSource>, PropertyField, TypeModel<TTarget>, PropertyField> BindingFactory(TypeModel sourceModel, TypeModel targetModel, IReadOnlyList<ModelFieldBinding<PropertyField, PropertyField>> fieldBindings)
		{
			return new DataModelBinding<TypeModel<TSource>, PropertyField, TypeModel<TTarget>, PropertyField>(
				(TypeModel<TSource>)sourceModel, (TypeModel<TTarget>)targetModel, fieldBindings
				);
		}

		private struct TryConvertDelegateFactory<TFrom, TTo> : IBindingTransformFactory
		{
			private TryConvertDelegate<TFrom, TTo> _tryConvertDelegate;

			public TryConvertDelegateFactory(TryConvertDelegate<TFrom, TTo> tryConvertDelegate)
			{
				_tryConvertDelegate = tryConvertDelegate;
			}

			public BindingTransformation CreateTransformation<TSource1, TTarget1>(
				IModelFieldOf<TSource1> sourceField, IModelFieldOf<TTarget1> targetField)
			{
				var convertDelegate = _tryConvertDelegate;
				return (BindingContext context, object @in, out object @out) =>
				{
					var result = convertDelegate((TFrom)@in, out var convertedValue);
					@out = convertedValue;
					return result;
				};
			}
		}
	}
}
