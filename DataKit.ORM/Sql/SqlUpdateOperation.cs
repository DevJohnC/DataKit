using DataKit.Mapping.Binding;
using DataKit.Modelling.TypeModels;
using DataKit.ORM.Sql.Expressions;
using DataKit.ORM.Sql.QueryBuilding;
using DataKit.SQL.Providers;
using DataKit.SQL.QueryExpressions;
using System;
using System.Linq.Expressions;

namespace DataKit.ORM.Sql
{
	public class SqlUpdateOperation<TEntity> : SqlDataOperation<TEntity>, IAssignmentQueryBuilder<TEntity>, IWhereQueryBuilder<TEntity>
		where TEntity : class
	{
		public SqlUpdateOperation(SqlDataSet<TEntity> dataSet, IQueryProvider queryProvider) : base(dataSet, queryProvider)
		{
			_queryBuilder = new SqlUpdateBuilder<TEntity>(DataSet.DataModel);
		}

		private readonly SqlUpdateBuilder<TEntity> _queryBuilder;

		ConditionBuilder<TEntity> IWhereQueryBuilder<TEntity>.WhereCondition => ((IWhereQueryBuilder<TEntity>)_queryBuilder).WhereCondition;

		FieldAssignmentBuilder<TEntity> IAssignmentQueryBuilder<TEntity>.Assignments => ((IAssignmentQueryBuilder<TEntity>)_queryBuilder).Assignments;

		public SqlUpdateOperation<TEntity> Table(string tableName)
		{
			_queryBuilder.Table(tableName);
			return this;
		}

		IQueryBuilder<TEntity> IQueryBuilder<TEntity>.Table(string tableName)
			=> Table(tableName);

		public SqlUpdateOperation<TEntity> AndWhere(SqlValueExpression<TEntity, bool> conditionExpression)
		{
			_queryBuilder.AndWhere(conditionExpression);
			return this;
		}

		public SqlUpdateOperation<TEntity> AndWhere(Expression<Func<TEntity, bool>> conditionExpression)
		{
			_queryBuilder.AndWhere(conditionExpression);
			return this;
		}

		public SqlUpdateOperation<TEntity> OrWhere(SqlValueExpression<TEntity, bool> conditionExpression)
		{
			_queryBuilder.OrWhere(conditionExpression);
			return this;
		}

		public SqlUpdateOperation<TEntity> OrWhere(Expression<Func<TEntity, bool>> conditionExpression)
		{
			_queryBuilder.OrWhere(conditionExpression);
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

		public SqlUpdateOperation<TEntity> AndWhere<TValue>(Schema.Sql.SqlStorageField<TEntity, TValue> field, ComparisonOperator comparisonType, TValue value)
		{
			_queryBuilder.AndWhere(field, comparisonType, value);
			return this;
		}

		public SqlUpdateOperation<TEntity> OrWhere<TValue>(Schema.Sql.SqlStorageField<TEntity, TValue> field, ComparisonOperator comparisonType, TValue value)
		{
			_queryBuilder.OrWhere(field, comparisonType, value);
			return this;
		}

		IWhereQueryBuilder<TEntity> IWhereQueryBuilder<TEntity>.AndWhere<TValue>(Schema.Sql.SqlStorageField<TEntity, TValue> field, ComparisonOperator comparisonType, TValue value)
			=> AndWhere(field, comparisonType, value);

		IWhereQueryBuilder<TEntity> IWhereQueryBuilder<TEntity>.OrWhere<TValue>(Schema.Sql.SqlStorageField<TEntity, TValue> field, ComparisonOperator comparisonType, TValue value)
			=> OrWhere(field, comparisonType, value);

		public SqlUpdateOperation<TEntity> Set<TProperty>(Expression<Func<TEntity, TProperty>> fieldSelector, TProperty value)
		{
			_queryBuilder.Set(fieldSelector, value);
			return this;
		}

		public SqlUpdateOperation<TEntity> Set<TProperty>(Expression<Func<TEntity, TProperty>> fieldSelector, Expression<Func<TEntity, TProperty>> valueExpression)
		{
			_queryBuilder.Set(fieldSelector, valueExpression);
			return this;
		}

		public SqlUpdateOperation<TEntity> Set(TEntity entity)
		{
			_queryBuilder.Set(entity);
			return this;
		}

		public SqlUpdateOperation<TEntity> Set<TView>(
			TView entityView,
			DataModelBinding<TypeModel<TView>, PropertyField, TypeModel<TEntity>, PropertyField> entityBinding = null
			)
			where TView : class
		{
			_queryBuilder.Set(entityView, entityBinding);
			return this;
		}

		IAssignmentQueryBuilder<TEntity> IAssignmentQueryBuilder<TEntity>.Set(TEntity entity)
			=> Set(entity);

		IAssignmentQueryBuilder<TEntity> IAssignmentQueryBuilder<TEntity>.Set<TView>(
			TView entityView,
			DataModelBinding<TypeModel<TView>, PropertyField, TypeModel<TEntity>, PropertyField> entityBinding
			)
			=> Set(entityView, entityBinding);

		IAssignmentQueryBuilder<TEntity> IAssignmentQueryBuilder<TEntity>.Set<TProperty>(Expression<Func<TEntity, TProperty>> fieldSelector, TProperty value)
			=> Set(fieldSelector, value);

		IAssignmentQueryBuilder<TEntity> IAssignmentQueryBuilder<TEntity>.Set<TProperty>(Expression<Func<TEntity, TProperty>> fieldSelector, Expression<Func<TEntity, TProperty>> valueExpression)
			=> Set(fieldSelector, valueExpression);

		protected override ExecutableQueryExpression BuildQuery()
			=> _queryBuilder.BuildQuery();
	}
}
