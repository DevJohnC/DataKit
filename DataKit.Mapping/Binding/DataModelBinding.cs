using System;
using System.Collections.Generic;
using System.Linq;
using DataKit.Modelling;
using DataKit.Modelling.TypeModels;

namespace DataKit.Mapping.Binding
{
	public abstract class DataModelBinding
	{
		protected DataModelBinding(IDataModel sourceModel, IDataModel targetModel, IReadOnlyList<ModelFieldBinding> fieldBindings)
		{
			SourceModel = sourceModel ?? throw new ArgumentNullException(nameof(sourceModel));
			TargetModel = targetModel ?? throw new ArgumentNullException(nameof(targetModel));
			FieldBindings = fieldBindings ?? throw new ArgumentNullException(nameof(fieldBindings));
		}

		public IDataModel SourceModel { get; }

		public IDataModel TargetModel { get; }

		public IReadOnlyList<ModelFieldBinding> FieldBindings { get; }
	}

	public abstract class DataModelBinding<TSourceField, TTargetField> : DataModelBinding
		where TSourceField : IModelField
		where TTargetField : IModelField
	{
		public new IReadOnlyList<ModelFieldBinding<TSourceField, TTargetField>> FieldBindings { get; }

		protected DataModelBinding(
			IDataModel sourceModel,
			IDataModel targetModel,
			IReadOnlyList<ModelFieldBinding<TSourceField, TTargetField>> fieldBindings) :
			base(sourceModel, targetModel, fieldBindings)
		{
			FieldBindings = fieldBindings ?? throw new ArgumentNullException(nameof(fieldBindings));
		}
	}

	public class DataModelBinding<TSourceModel, TSourceField, TTargetModel, TTargetField> : DataModelBinding<TSourceField, TTargetField>
		where TSourceModel : IDataModel<TSourceField>
		where TTargetModel : IDataModel<TTargetField>
		where TSourceField : IModelField
		where TTargetField : IModelField
	{
		public DataModelBinding(
			TSourceModel sourceModel,
			TTargetModel targetModel, 
			IReadOnlyList<ModelFieldBinding<TSourceField, TTargetField>> fieldBindings) :
			base(sourceModel, targetModel, fieldBindings)
		{
			SourceModel = sourceModel;
			TargetModel = targetModel;
		}

		public new TSourceModel SourceModel { get; }

		public new TTargetModel TargetModel { get; }
	}

	public static class DataModelBindingExtensions
	{
		//  overload with mismatched model on the SourceModel side
		public static DataModelBinding<TSourceModel, TSourceField, TTargetModel, TTargetField>
			RouteBindingFromSourceType<TSourceModel, TSourceField, TIntermediateModel, TTargetModel, TTargetField>(
			this DataModelBinding<TSourceModel, TSourceField, TIntermediateModel, TTargetField> sourceToIntermediateBinding,
			DataModelBinding<TIntermediateModel, TTargetField, TTargetModel, TTargetField> intermediateToTargetBinding
			)
			where TSourceField : IModelField
			where TTargetField : IModelField
			where TSourceModel : IDataModel<TSourceField>
			where TIntermediateModel : TypeModel, IDataModel<TTargetField>
			where TTargetModel : IDataModel<TTargetField>
		{
			//  this method needs TIntermediateModel to be a TypeModel
			//  a TypeModel is required because we're performing a transform on
			//  an already transformed value
			//  for now the method is left without a generic constraint on TIntermediateModel
			//  forcing the use of TypeModel so that I can come back to it
			//  as needed at a later date.
			//  in the future, consider allowing the consumer to pass in a binding
			//  that binds TIntermediate to a TypeModel that can be used to transform
			//  bound objects

			var fieldBindings = new List<ModelFieldBinding<TSourceField, TTargetField>>();

			//  loop each field bound from source->intermediate
			foreach (var sourceBinding in sourceToIntermediateBinding.FieldBindings)
			{
				//  find a matching binding from intermediate->target
				var targetBinding = intermediateToTargetBinding.FieldBindings
					.FirstOrDefault(q => q.BindingSource.Path.SequenceEqual(sourceBinding.BindingTarget.Path));

				if (targetBinding != null)
				{
					//  copy binding
					var routedBinding = new ModelFieldBinding<TSourceField, TTargetField>(
						sourceBinding.BindingSource,
						targetBinding.BindingTarget,
						new CompositeBindingTransformFactory(sourceBinding.Transformation, targetBinding.Transformation)
							.CreateTransformation(sourceBinding.SourceField, targetBinding.TargetField)
						);
					fieldBindings.Add(routedBinding);

					continue;
				}

				var deeperModel = sourceBinding.TargetField.FieldModel;
				if (deeperModel == null)
					continue;

				var runOnceSourceTransform = RunOnceTransformation.Create(sourceBinding.Transformation);
				BindIntermediateFields(runOnceSourceTransform, sourceBinding, deeperModel, new IModelField[0], sourceBinding.BindingTarget.Path);
			}

			return new DataModelBinding<TSourceModel, TSourceField, TTargetModel, TTargetField>(
				sourceToIntermediateBinding.SourceModel,
				intermediateToTargetBinding.TargetModel,
				fieldBindings
				);

			void BindIntermediateFields(BindingTransformation sourceTransform, ModelFieldBinding<TSourceField, TTargetField> sourceBinding, IDataModel model, IEnumerable<IModelField> pathOfFields, string[] sourceBindingPath)
			{
				foreach (var field in model.Fields)
				{
					var thisPathOfFields = pathOfFields.Concat(new[] { field });
					var intermediateBindingPath = sourceBindingPath.Concat(thisPathOfFields.Select(q => q.FieldName))
						.ToArray();
					var targetBinding = intermediateToTargetBinding.FieldBindings
						.FirstOrDefault(q => q.BindingSource.Path.SequenceEqual(intermediateBindingPath));
					if (targetBinding != null)
					{
						var routedBinding = new ModelFieldBinding<TSourceField, TTargetField>(
							sourceBinding.BindingSource,
							targetBinding.BindingTarget,
							new CompositeBindingTransformFactory(
								sourceTransform,
								ReadFieldTransform.Create(thisPathOfFields.OfType<PropertyField>().ToArray()),
								targetBinding.Transformation)
								.CreateTransformation(sourceBinding.SourceField, targetBinding.TargetField)
							);
						fieldBindings.Add(routedBinding);

						continue;
					}

					var deeperModel = field.FieldModel;
					if (deeperModel == null)
						continue;

					BindIntermediateFields(sourceTransform, sourceBinding, deeperModel, thisPathOfFields, sourceBinding.BindingTarget.Path);
				}
			}
		}

		//  overload with a mismatched model on the TargetModel side
		public static DataModelBinding<TSourceModel, TSourceField, TTargetModel, TTargetField>
			RouteBindingToTargetType<TSourceModel, TSourceField, TIntermediateModel, TTargetModel, TTargetField>(
			this DataModelBinding<TSourceModel, TSourceField, TIntermediateModel, TSourceField> sourceToIntermediateBinding,
			DataModelBinding<TIntermediateModel, TSourceField, TTargetModel, TTargetField> intermediateToTargetBinding
			)
			where TSourceField : IModelField
			where TTargetField : IModelField
			where TSourceModel : IDataModel<TSourceField>
			where TIntermediateModel : TypeModel, IDataModel<TSourceField>
			where TTargetModel : IDataModel<TTargetField>
		{
			//  this method needs TIntermediateModel to be a TypeModel
			//  a TypeModel is required because we're performing a transform on
			//  an already transformed value
			//  for now the method is left without a generic constraint on TIntermediateModel
			//  forcing the use of TypeModel so that I can come back to it
			//  as needed at a later date.
			//  in the future, consider allowing the consumer to pass in a binding
			//  that binds TIntermediate to a TypeModel that can be used to transform
			//  bound objects

			var fieldBindings = new List<ModelFieldBinding<TSourceField, TTargetField>>();

			//  loop each field bound from source->intermediate
			foreach (var sourceBinding in sourceToIntermediateBinding.FieldBindings)
			{
				//  find a matching binding from intermediate->target
				var targetBinding = intermediateToTargetBinding.FieldBindings
					.FirstOrDefault(q => q.BindingSource.Path.SequenceEqual(sourceBinding.BindingTarget.Path));

				if (targetBinding != null)
				{
					//  copy binding
					var routedBinding = new ModelFieldBinding<TSourceField, TTargetField>(
						sourceBinding.BindingSource,
						targetBinding.BindingTarget,
						new CompositeBindingTransformFactory(sourceBinding.Transformation, targetBinding.Transformation)
							.CreateTransformation(sourceBinding.SourceField, targetBinding.TargetField)
						);
					fieldBindings.Add(routedBinding);

					continue;
				}

				var deeperModel = sourceBinding.TargetField.FieldModel;
				if (deeperModel == null)
					continue;

				var runOnceSourceTransform = RunOnceTransformation.Create(sourceBinding.Transformation);
				BindIntermediateFields(runOnceSourceTransform, sourceBinding, deeperModel, new IModelField[0], sourceBinding.BindingTarget.Path);
			}

			return new DataModelBinding<TSourceModel, TSourceField, TTargetModel, TTargetField>(
				sourceToIntermediateBinding.SourceModel,
				intermediateToTargetBinding.TargetModel,
				fieldBindings
				);

			void BindIntermediateFields(BindingTransformation sourceTransform, ModelFieldBinding<TSourceField, TSourceField> sourceBinding, IDataModel model, IEnumerable<IModelField> pathOfFields, string[] sourceBindingPath)
			{
				foreach (var field in model.Fields)
				{
					var thisPathOfFields = pathOfFields.Concat(new[] { field });
					var intermediateBindingPath = sourceBindingPath.Concat(thisPathOfFields.Select(q => q.FieldName))
						.ToArray();
					var targetBinding = intermediateToTargetBinding.FieldBindings
						.FirstOrDefault(q => q.BindingSource.Path.SequenceEqual(intermediateBindingPath));
					if (targetBinding != null)
					{
						var routedBinding = new ModelFieldBinding<TSourceField, TTargetField>(
							sourceBinding.BindingSource,
							targetBinding.BindingTarget,
							new CompositeBindingTransformFactory(
								sourceTransform,
								ReadFieldTransform.Create(thisPathOfFields.OfType<PropertyField>().ToArray()),
								targetBinding.Transformation)
								.CreateTransformation(sourceBinding.SourceField, targetBinding.TargetField)
							);
						fieldBindings.Add(routedBinding);

						continue;
					}

					var deeperModel = field.FieldModel;
					if (deeperModel == null)
						continue;

					BindIntermediateFields(sourceTransform, sourceBinding, deeperModel, thisPathOfFields, sourceBinding.BindingTarget.Path);
				}
			}
		}
	}

}
