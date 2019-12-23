using DataKit.ORM.Schema.Sql;
using DataKit.ORM.Sql.Expressions;
using System;
using System.Linq.Expressions;

namespace DataKit.ORM.Sql.QueryBuilding
{
	public interface IWhereQueryBuilder<TEntity> : IQueryBuilder<TEntity>
		where TEntity : class
	{
		ConditionBuilder<TEntity> WhereCondition { get; }

		IWhereQueryBuilder<TEntity> AndWhere(SqlValueExpression<TEntity, bool> conditionExpression);

		IWhereQueryBuilder<TEntity> AndWhere(Expression<Func<TEntity, bool>> conditionExpression);

		IWhereQueryBuilder<TEntity> AndWhere<TValue>(SqlStorageField<TEntity, TValue> field, SqlComparisonOperator comparisonType, TValue value);

		IWhereQueryBuilder<TEntity> OrWhere(SqlValueExpression<TEntity, bool> conditionExpression);

		IWhereQueryBuilder<TEntity> OrWhere(Expression<Func<TEntity, bool>> conditionExpression);

		IWhereQueryBuilder<TEntity> OrWhere<TValue>(SqlStorageField<TEntity, TValue> field, SqlComparisonOperator comparisonType, TValue value);
	}
}
