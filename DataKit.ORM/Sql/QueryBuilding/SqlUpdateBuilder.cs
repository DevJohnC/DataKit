using DataKit.Mapping.Binding;
using DataKit.Modelling.TypeModels;
using DataKit.ORM.Schema.Sql;
using DataKit.ORM.Sql.Expressions;
using Silk.Data.SQL.Expressions;
using System;
using System.Linq.Expressions;

namespace DataKit.ORM.Sql.QueryBuilding
{
	public class SqlUpdateBuilder<TEntity> :
		SqlQueryBuilder<TEntity>, IAssignmentQueryBuilder<TEntity>, IWhereQueryBuilder<TEntity>, IAliasIdentifier
		where TEntity : class
	{
		private string _tableName;

		string IAliasIdentifier.AliasIdentifier => _tableName;

		private FieldAssignmentBuilder<TEntity> _assignments;
		FieldAssignmentBuilder<TEntity> IAssignmentQueryBuilder<TEntity>.Assignments => _assignments;

		private ConditionBuilder<TEntity> _whereBuilder;
		ConditionBuilder<TEntity> IWhereQueryBuilder<TEntity>.WhereCondition => _whereBuilder;

		public SqlUpdateBuilder(SqlDataModel<TEntity> dataModel) :
			base(dataModel)
		{
			_tableName = DataModel.StorageModel.DefaultTableName;
			_whereBuilder = new ConditionBuilder<TEntity>(dataModel, ConditionConverter);
			_assignments = new FieldAssignmentBuilder<TEntity>(dataModel);
		}

		public SqlUpdateBuilder<TEntity> Table(string tableName)
		{
			_tableName = tableName;
			return this;
		}

		IQueryBuilder<TEntity> IQueryBuilder<TEntity>.Table(string tableName)
			=> Table(tableName);

		public override QueryExpression BuildQuery()
		{
			var where = _whereBuilder.Build();
			var row = _assignments.Build();

			if (where?.RequiresJoins == true)
				throw new InvalidOperationException("Expressions that require joins aren't valid for DELETE statements.");

			return QueryExpression.Update(
				QueryExpression.Table(_tableName),
				where: where?.QueryExpression,
				assignments: row
				);
		}

		public SqlUpdateBuilder<TEntity> AndWhere(SqlValueExpression<TEntity, bool> conditionExpression)
		{
			_whereBuilder.AndAlso(conditionExpression);
			return this;
		}

		public SqlUpdateBuilder<TEntity> AndWhere(Expression<Func<TEntity, bool>> conditionExpression)
		{
			_whereBuilder.AndAlso(conditionExpression);
			return this;
		}

		public SqlUpdateBuilder<TEntity> OrWhere(SqlValueExpression<TEntity, bool> conditionExpression)
		{
			_whereBuilder.OrElse(conditionExpression);
			return this;
		}

		public SqlUpdateBuilder<TEntity> OrWhere(Expression<Func<TEntity, bool>> conditionExpression)
		{
			_whereBuilder.OrElse(conditionExpression);
			return this;
		}

		IWhereQueryBuilder<TEntity> IWhereQueryBuilder<TEntity>.AndWhere(SqlValueExpression<TEntity, bool> conditionExpression)
			=> AndWhere(conditionExpression);

		IWhereQueryBuilder<TEntity> IWhereQueryBuilder<TEntity>.AndWhere(Expression<Func<TEntity, bool>> conditionExpression)
			=> AndWhere(conditionExpression);

		IWhereQueryBuilder<TEntity> IWhereQueryBuilder<TEntity>.OrWhere(SqlValueExpression<TEntity, bool> conditionExpression)
			=> OrWhere(conditionExpression);

		IWhereQueryBuilder<TEntity> IWhereQueryBuilder<TEntity>.OrWhere(Expression<Func<TEntity, bool>> conditionExpression)
			=> OrWhere(conditionExpression);

		public SqlUpdateBuilder<TEntity> AndWhere<TValue>(SqlStorageField<TEntity, TValue> field, ComparisonOperator comparisonType, TValue value)
		{
			_whereBuilder.AndAlso(field, comparisonType, value);
			return this;
		}

		public SqlUpdateBuilder<TEntity> OrWhere<TValue>(SqlStorageField<TEntity, TValue> field, ComparisonOperator comparisonType, TValue value)
		{
			_whereBuilder.OrElse(field, comparisonType, value);
			return this;
		}

		IWhereQueryBuilder<TEntity> IWhereQueryBuilder<TEntity>.AndWhere<TValue>(SqlStorageField<TEntity, TValue> field, ComparisonOperator comparisonType, TValue value)
			=> AndWhere(field, comparisonType, value);

		IWhereQueryBuilder<TEntity> IWhereQueryBuilder<TEntity>.OrWhere<TValue>(SqlStorageField<TEntity, TValue> field, ComparisonOperator comparisonType, TValue value)
			=> OrWhere(field, comparisonType, value);

		public SqlUpdateBuilder<TEntity> Set<TProperty>(Expression<Func<TEntity, TProperty>> fieldSelector, TProperty value)
		{
			_assignments.Set(fieldSelector, value);
			return this;
		}

		public SqlUpdateBuilder<TEntity> Set<TProperty>(Expression<Func<TEntity, TProperty>> fieldSelector, Expression<Func<TEntity, TProperty>> valueExpression)
		{
			var sqlValueExpression = ValueConverter.ConvertExpression(valueExpression);
			if (sqlValueExpression.RequiresJoins)
				throw new InvalidOperationException("Expressions that require joins aren't valid for INSERT statements.");
			_assignments.Set(fieldSelector, sqlValueExpression);
			return this;
		}

		public SqlUpdateBuilder<TEntity> Set(TEntity entity)
		{
			_assignments.SetAll(entity);
			return this;
		}

		public SqlUpdateBuilder<TEntity> Set<TView>(
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
	}
}
