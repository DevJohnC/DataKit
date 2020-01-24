using DataKit.Mapping.Binding;
using DataKit.Modelling.TypeModels;
using DataKit.ORM.Schema.Sql;
using DataKit.ORM.Sql.Expressions;
using DataKit.SQL.QueryExpressions;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace DataKit.ORM.Sql.QueryBuilding
{
	public class SqlInsertBuilder<TEntity> : SqlQueryBuilder<TEntity>, IAssignmentQueryBuilder<TEntity>, IAliasIdentifier
		where TEntity : class
	{
		private string _tableName;

		private FieldAssignmentBuilder<TEntity> _assignments;
		FieldAssignmentBuilder<TEntity> IAssignmentQueryBuilder<TEntity>.Assignments => _assignments;

		string IAliasIdentifier.AliasIdentifier => _tableName;

		public SqlInsertBuilder(SqlDataModel<TEntity> dataModel) :
			base(dataModel)
		{
			_tableName = DataModel.StorageModel.DefaultTableName;
			_assignments = new FieldAssignmentBuilder<TEntity>(dataModel);
			PopulateDefaults();
		}

		private void PopulateDefaults()
		{
			foreach (var storageField in DataModel.StorageModel.Fields
				.Where(q => !q.IsServerGenerated && q.HasDefaultValue && !q.RequiresJoin))
			{
				_assignments.SetDefault(storageField);
			}
		}

		public SqlInsertBuilder<TEntity> Table(string tableName)
		{
			_tableName = tableName;
			return this;
		}

		IQueryBuilder<TEntity> IQueryBuilder<TEntity>.Table(string tableName)
			=> Table(tableName);

		public SqlInsertBuilder<TEntity> Set<TProperty>(Expression<Func<TEntity, TProperty>> fieldSelector, TProperty value)
		{
			_assignments.Set(fieldSelector, value);
			return this;
		}

		public SqlInsertBuilder<TEntity> Set<TProperty>(Expression<Func<TEntity, TProperty>> fieldSelector, Expression<Func<TEntity, TProperty>> valueExpression)
		{
			var sqlValueExpression = ValueConverter.ConvertExpression(valueExpression);
			if (sqlValueExpression.RequiresJoins)
				throw new InvalidOperationException("Expressions that require joins aren't valid for INSERT statements.");
			_assignments.Set(fieldSelector, sqlValueExpression);
			return this;
		}

		public SqlInsertBuilder<TEntity> Set(TEntity entity)
		{
			_assignments.SetAll(entity);
			return this;
		}

		public SqlInsertBuilder<TEntity> Set<TView>(
			TView entityView,
			DataModelBinding<TypeModel<TView>, PropertyField, TypeModel<TEntity>, PropertyField> entityBinding
			)
			where TView : class
		{
			_assignments.SetAll(entityView, entityBinding);
			return this;
		}

		IAssignmentQueryBuilder<TEntity> IAssignmentQueryBuilder<TEntity>.Set<TProperty>(Expression<Func<TEntity, TProperty>> fieldSelector, TProperty value)
			=> Set(fieldSelector, value);

		IAssignmentQueryBuilder<TEntity> IAssignmentQueryBuilder<TEntity>.Set<TProperty>(Expression<Func<TEntity, TProperty>> fieldSelector, Expression<Func<TEntity, TProperty>> valueExpression)
			=> Set(fieldSelector, valueExpression);

		IAssignmentQueryBuilder<TEntity> IAssignmentQueryBuilder<TEntity>.Set(TEntity entity)
			=> Set(entity);

		IAssignmentQueryBuilder<TEntity> IAssignmentQueryBuilder<TEntity>.Set<TView>(
			TView entityView,
			DataModelBinding<TypeModel<TView>, PropertyField, TypeModel<TEntity>, PropertyField> entityBinding
			)
			=> Set(entityView, entityBinding);

		public override ExecutableQueryExpression BuildQuery()
		{
			var assignments = _assignments.Build();

			return QueryExpression.Insert(
				QueryExpression.Table(_tableName),
				assignments.Columns,
				assignments.Row
				);
		}
	}
}
