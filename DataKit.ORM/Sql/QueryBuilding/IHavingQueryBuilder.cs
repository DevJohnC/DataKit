using DataKit.ORM.Schema.Sql;
using DataKit.ORM.Sql.Expressions;
using System;
using System.Linq.Expressions;

namespace DataKit.ORM.Sql.QueryBuilding
{
	public interface IHavingQueryBuilder<TEntity> : IQueryBuilder<TEntity>
		where TEntity : class
	{
		ConditionBuilder<TEntity> HavingCondition { get; }

		IHavingQueryBuilder<TEntity> AndHaving(SqlValueExpression<TEntity, bool> conditionExpression);

		IHavingQueryBuilder<TEntity> AndHaving(Expression<Func<TEntity, bool>> conditionExpression);

		IHavingQueryBuilder<TEntity> AndHaving<TValue>(SqlStorageField<TEntity, TValue> field, SqlComparisonOperator comparisonType, TValue value);

		IHavingQueryBuilder<TEntity> OrHaving(SqlValueExpression<TEntity, bool> conditionExpression);

		IHavingQueryBuilder<TEntity> OrHaving(Expression<Func<TEntity, bool>> conditionExpression);

		IHavingQueryBuilder<TEntity> OrHaving<TValue>(SqlStorageField<TEntity, TValue> field, SqlComparisonOperator comparisonType, TValue value);
	}
}
