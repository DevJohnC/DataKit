using DataKit.Mapping;
using DataKit.Mapping.Binding;
using DataKit.Modelling;
using DataKit.Modelling.TypeModels;
using DataKit.ORM.Schema.Sql;
using DataKit.ORM.Sql.Expressions;
using DataKit.SQL.QueryExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DataKit.ORM.Sql.QueryBuilding
{
	/// <summary>
	/// Builds ColumnAssignment QueryExpressions.
	/// </summary>
	public class FieldAssignmentBuilder<TEntity>
		where TEntity : class
	{
		private readonly SqlDataModel<TEntity> _dataModel;
		private readonly List<(ColumnIdentifierQueryExpression Column, QueryExpression ValueExpression)> _fieldAssignments
			= new List<(ColumnIdentifierQueryExpression Column, QueryExpression ValueExpression)>();

		public FieldAssignmentBuilder(SqlDataModel<TEntity> dataModel)
		{
			_dataModel = dataModel;
		}

		private object RunTransformation(BindingContext context, object value, BindingTransformation transformation)
		{
			if (transformation == null || !transformation(context, value, out var transformedValue))
				return value;
			return transformedValue;
		}

		private bool TryReadValue(
			BindingContext context,
			ModelFieldBinding<PropertyField,SqlStorageField<TEntity>> boundField,
			DataModelReaderNavigator<TypeModel<TEntity>, PropertyField> readerNavigator,
			out object readValue
			)
		{
			if (!readerNavigator.TryPrepareForRead(boundField.BindingSource))
			{
				readValue = default;
				return false;
			}

			var value = ((SqlEntityField<TEntity>)boundField.BindingSource.Field).ReadValue(readerNavigator.Reader);
			readValue = RunTransformation(context, value, boundField.Transformation);
			return true;
		}

		protected void AddFieldAssignment(ColumnIdentifierQueryExpression columnExpression, QueryExpression valueExpression)
		{
			_fieldAssignments.RemoveAll(q => q.Column.ColumnName == columnExpression.IdentifierName);
			_fieldAssignments.Add((columnExpression, valueExpression));
		}

		private IEnumerable<AssignColumnExpression> GetAssignColumnExpressions()
		{
			foreach (var fieldAssignment in _fieldAssignments)
			{
				yield return QueryExpression.Assign(fieldAssignment.Column.ColumnName, fieldAssignment.ValueExpression);
			}
		}

		public AssignColumnExpression[] Build()
			=> GetAssignColumnExpressions().ToArray();

		public void SetDefault(SqlStorageField storageField)
		{
			var defaultValueVisitor = new DefaultValueVisitor();
			var valueNode = defaultValueVisitor.Visit(storageField) as ValueNode;
			AddFieldAssignment(
				QueryExpression.Column(storageField.ColumnName),
				QueryExpression.Value(valueNode.Value)
				);
		}

		public void Set<TProperty>(Expression<Func<TEntity, TProperty>> fieldSelector, TProperty value)
		{
			var field = _dataModel.EntityModel.GetField(fieldSelector);
			if (field == null)
				throw new Exception("Couldn't resolve field on model.");

			//  get the field binding for writing to the db, the "in" binding
			var boundField = _dataModel.InBinding.GetFieldBinding(field.Field);
			if (boundField == null)
				throw new Exception("Couldn't resolve field binding.");

			AddFieldAssignment(
				QueryExpression.Column(boundField.TargetField.ColumnName),
				QueryExpression.Value(value)
				);
		}

		public void Set<TProperty>(
			Expression<Func<TEntity, TProperty>> fieldSelector,
			SqlValueExpression<TEntity, TProperty> valueExpression)
		{
			var field = _dataModel.EntityModel.GetField(fieldSelector);
			if (field == null)
				throw new Exception("Couldn't resolve field on model.");

			//  get the field binding for writing to the db, the "in" binding
			var boundField = _dataModel.InBinding.GetFieldBinding(field.Field);
			if (boundField == null)
				throw new Exception("Couldn't resolve field binding.");

			AddFieldAssignment(
				QueryExpression.Column(boundField.TargetField.ColumnName),
				valueExpression.QueryExpression
				);
		}

		public void SetAll(TEntity entity)
		{
			var entityReader = new ObjectDataModelReader<TEntity>(entity);
			var entityNavigator = new DataModelReaderNavigator<TypeModel<TEntity>, PropertyField>(entityReader);
			var bindingContext = new BindingContext();

			foreach (var storageField in _dataModel.StorageModel.Fields.Where(q => !q.RequiresJoin))
			{
				if (storageField.IsServerGenerated)
					continue;

				var boundField = _dataModel.InBinding.GetFieldBinding(storageField);
				if (boundField == null)
					throw new Exception("Couldn't resolve field binding.");

				if (TryReadValue(bindingContext, boundField, entityNavigator, out var value))
				{
					AddFieldAssignment(
						QueryExpression.Column(storageField.ColumnName),
						QueryExpression.Value(value)
						);
				}
			}
		}

		private void CopyBoundValues<TView>(
			BindingContext context,
			DataModelReaderNavigator<TypeModel<TView>, PropertyField> readerNavigator,
			DataModelBinding<TypeModel<TView>, PropertyField, SqlStorageModel<TEntity>, SqlStorageField<TEntity>> routedBinding
			)
			where TView : class
		{
			foreach (var fieldBinding in routedBinding.FieldBindings.Where(q => !q.TargetField.RequiresJoin && !q.TargetField.IsServerGenerated))
			{
				var viewReaderConverter = new ConvertorVisitor<TView>(readerNavigator.Reader);
				readerNavigator.TryPrepareForRead(fieldBinding.BindingSource);
				var readValueNode = viewReaderConverter.Visit(fieldBinding.SourceField) as ValueNode;
				var boundValue = RunTransformation(context, readValueNode.Value, fieldBinding.Transformation);

				AddFieldAssignment(
					QueryExpression.Column(fieldBinding.TargetField.ColumnName),
					QueryExpression.Value(boundValue)
				);
			}
		}

		public void SetAll<TView>(
			TView entityView,
			DataModelBinding<TypeModel<TView>, PropertyField, TypeModel<TEntity>, PropertyField> entityBinding
			) where TView : class
		{
			if (entityView == null)
				throw new ArgumentNullException(nameof(entityView));
			if (entityBinding == null)
				throw new ArgumentNullException(nameof(entityBinding));

			SetAll(entityView, _dataModel.GetInStorageBinding(entityBinding));
		}

		public void SetAll<TView>(
			TView entityView,
			DataModelBinding<TypeModel<TView>, PropertyField, SqlStorageModel<TEntity>, SqlStorageField<TEntity>> storageBinding
			) where TView : class
		{
			if (entityView == null)
				throw new ArgumentNullException(nameof(entityView));
			if (storageBinding == null)
				throw new ArgumentNullException(nameof(storageBinding));

			var viewReader = new ObjectDataModelReader<TView>(entityView);
			var viewNavigator = new DataModelReaderNavigator<TypeModel<TView>, PropertyField>(viewReader);
			var bindingContext = new BindingContext();

			CopyBoundValues(bindingContext, viewNavigator, storageBinding);
		}

		private class ConvertorVisitor<TView> : ModelVisitor
			where TView : class
		{
			private readonly IDataModelReader<TypeModel<TView>, PropertyField> _reader;

			public ConvertorVisitor(IDataModelReader<TypeModel<TView>, PropertyField> reader)
			{
				_reader = reader;
			}

			public override IModelNode VisitField<T>(IModelFieldOf<T> field)
			{
				return new ValueNode { Value = _reader.ReadField<T>(field as PropertyField) };
			}
		}

		private class DefaultValueVisitor : ModelVisitor
		{
			public override IModelNode VisitField<T>(IModelFieldOf<T> field)
			{
				return new ValueNode
				{
					Value = ((SqlStorageField<TEntity, T>)field).DefaultValue()
				};
			}
		}

		private class ValueNode : IModelNode
		{
			public object Value { get; set; }

			public IModelNode Accept(IModelVisitor visitor)
			{
				return visitor.VisitExtension(this);
			}
		}
	}
}
