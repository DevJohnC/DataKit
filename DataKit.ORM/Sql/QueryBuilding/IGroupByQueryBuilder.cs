using DataKit.ORM.Schema.Sql;
using DataKit.ORM.Sql.Expressions;
using System;
using System.Linq.Expressions;

namespace DataKit.ORM.Sql.QueryBuilding
{
	public interface IGroupByQueryBuilder<TEntity> : IQueryBuilder<TEntity>
		where TEntity : class
	{
		GroupByBuilder<TEntity> GroupByClause { get; }

		IGroupByQueryBuilder<TEntity> GroupBy<TValue>(SqlValueExpression<TEntity, TValue> expression);

		IGroupByQueryBuilder<TEntity> GroupBy<TValue>(Expression<Func<TEntity, TValue>> expression);

		IGroupByQueryBuilder<TEntity> GroupBy<TValue>(SqlStorageField<TEntity, TValue> field);
	}
}
