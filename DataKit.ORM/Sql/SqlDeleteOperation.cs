using System;
using System.Linq.Expressions;
using DataKit.ORM.Sql.Expressions;
using DataKit.ORM.Sql.QueryBuilding;
using DataKit.SQL.Providers;
using DataKit.SQL.QueryExpressions;

namespace DataKit.ORM.Sql
{
	public class SqlDeleteOperation<TEntity> : SqlDataOperation<TEntity>, IWhereQueryBuilder<TEntity>
		where TEntity : class
	{
		public SqlDeleteOperation(SqlDataSet<TEntity> dataSet, IQueryProvider queryProvider) : base(dataSet, queryProvider)
		{
			_queryBuilder = new SqlDeleteBuilder<TEntity>(DataSet.DataModel);
		}

		private readonly SqlDeleteBuilder<TEntity> _queryBuilder;

		ConditionBuilder<TEntity> IWhereQueryBuilder<TEntity>.WhereCondition => ((IWhereQueryBuilder<TEntity>)_queryBuilder).WhereCondition;

		public SqlDeleteOperation<TEntity> Table(string tableName)
		{
			_queryBuilder.Table(tableName);
			return this;
		}

		IQueryBuilder<TEntity> IQueryBuilder<TEntity>.Table(string tableName)
			=> Table(tableName);

		public SqlDeleteOperation<TEntity> AndWhere(SqlValueExpression<TEntity, bool> conditionExpression)
		{
			_queryBuilder.AndWhere(conditionExpression);
			return this;
		}

		public SqlDeleteOperation<TEntity> AndWhere(Expression<Func<TEntity, bool>> conditionExpression)
		{
			_queryBuilder.AndWhere(conditionExpression);
			return this;
		}

		public SqlDeleteOperation<TEntity> OrWhere(SqlValueExpression<TEntity, bool> conditionExpression)
		{
			_queryBuilder.OrWhere(conditionExpression);
			return this;
		}

		public SqlDeleteOperation<TEntity> OrWhere(Expression<Func<TEntity, bool>> conditionExpression)
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

		public SqlDeleteOperation<TEntity> AndWhere<TValue>(Schema.Sql.SqlStorageField<TEntity, TValue> field, SqlComparisonOperator comparisonType, TValue value)
		{
			_queryBuilder.AndWhere(field, comparisonType, value);
			return this;
		}

		public SqlDeleteOperation<TEntity> OrWhere<TValue>(Schema.Sql.SqlStorageField<TEntity, TValue> field, SqlComparisonOperator comparisonType, TValue value)
		{
			_queryBuilder.OrWhere(field, comparisonType, value);
			return this;
		}

		IWhereQueryBuilder<TEntity> IWhereQueryBuilder<TEntity>.AndWhere<TValue>(Schema.Sql.SqlStorageField<TEntity, TValue> field, SqlComparisonOperator comparisonType, TValue value)
			=> AndWhere(field, comparisonType, value);

		IWhereQueryBuilder<TEntity> IWhereQueryBuilder<TEntity>.OrWhere<TValue>(Schema.Sql.SqlStorageField<TEntity, TValue> field, SqlComparisonOperator comparisonType, TValue value)
			=> OrWhere(field, comparisonType, value);

		protected override ExecutableQueryExpression BuildQuery()
			=> _queryBuilder.BuildQuery();
	}
}
