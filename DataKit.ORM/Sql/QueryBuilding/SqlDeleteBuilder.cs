using System;
using System.Linq.Expressions;
using DataKit.ORM.Schema.Sql;
using DataKit.ORM.Sql.Expressions;
using Silk.Data.SQL.Expressions;

namespace DataKit.ORM.Sql.QueryBuilding
{
	public class SqlDeleteBuilder<TEntity> : SqlQueryBuilder<TEntity>, IWhereQueryBuilder<TEntity>, IAliasIdentifier
		where TEntity : class
	{
		private string _tableName;

		string IAliasIdentifier.AliasIdentifier => _tableName;

		private ConditionBuilder<TEntity> _whereBuilder;
		ConditionBuilder<TEntity> IWhereQueryBuilder<TEntity>.WhereCondition => _whereBuilder;

		public SqlDeleteBuilder(SqlDataModel<TEntity> dataModel) :
			base(dataModel)
		{
			_tableName = DataModel.StorageModel.DefaultTableName;
			_whereBuilder = new ConditionBuilder<TEntity>(dataModel, ConditionConverter);
		}

		public SqlDeleteBuilder<TEntity> Table(string tableName)
		{
			_tableName = tableName;
			return this;
		}

		IQueryBuilder<TEntity> IQueryBuilder<TEntity>.Table(string tableName)
			=> Table(tableName);

		public override QueryExpression BuildQuery()
		{
			var where = _whereBuilder.Build();

			if (where?.RequiresJoins == true)
				throw new InvalidOperationException("Expressions that require joins aren't valid for DELETE statements.");

			return QueryExpression.Delete(
				QueryExpression.Table(_tableName), where?.QueryExpression
				);
		}

		public SqlDeleteBuilder<TEntity> AndWhere(SqlValueExpression<TEntity, bool> conditionExpression)
		{
			_whereBuilder.AndAlso(conditionExpression);
			return this;
		}

		public SqlDeleteBuilder<TEntity> AndWhere(Expression<Func<TEntity, bool>> conditionExpression)
		{
			_whereBuilder.AndAlso(conditionExpression);
			return this;
		}

		public SqlDeleteBuilder<TEntity> OrWhere(SqlValueExpression<TEntity, bool> conditionExpression)
		{
			_whereBuilder.OrElse(conditionExpression);
			return this;
		}

		public SqlDeleteBuilder<TEntity> OrWhere(Expression<Func<TEntity, bool>> conditionExpression)
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

		public SqlDeleteBuilder<TEntity> AndWhere<TValue>(SqlStorageField<TEntity, TValue> field, ComparisonOperator comparisonType, TValue value)
		{
			_whereBuilder.AndAlso(field, comparisonType, value);
			return this;
		}

		public SqlDeleteBuilder<TEntity> OrWhere<TValue>(SqlStorageField<TEntity, TValue> field, ComparisonOperator comparisonType, TValue value)
		{
			_whereBuilder.OrElse(field, comparisonType, value);
			return this;
		}

		IWhereQueryBuilder<TEntity> IWhereQueryBuilder<TEntity>.AndWhere<TValue>(SqlStorageField<TEntity, TValue> field, ComparisonOperator comparisonType, TValue value)
			=> AndWhere(field, comparisonType, value);

		IWhereQueryBuilder<TEntity> IWhereQueryBuilder<TEntity>.OrWhere<TValue>(SqlStorageField<TEntity, TValue> field, ComparisonOperator comparisonType, TValue value)
			=> OrWhere(field, comparisonType, value);
	}
}
