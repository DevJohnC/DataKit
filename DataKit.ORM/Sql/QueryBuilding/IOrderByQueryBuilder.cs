using DataKit.ORM.Schema.Sql;
using DataKit.ORM.Sql.Expressions;
using System;
using System.Linq.Expressions;

namespace DataKit.ORM.Sql.QueryBuilding
{
	public interface IOrderByQueryBuilder<TEntity> : IQueryBuilder<TEntity>
		where TEntity : class
	{
		OrderByBuilder<TEntity> OrderByClause { get; }

		IOrderByQueryBuilder<TEntity> OrderBy<TValue>(SqlValueExpression<TEntity, TValue> expression);

		IOrderByQueryBuilder<TEntity> OrderBy<TValue>(Expression<Func<TEntity, TValue>> expression);

		IOrderByQueryBuilder<TEntity> OrderBy<TValue>(SqlStorageField<TEntity, TValue> field);

		IOrderByQueryBuilder<TEntity> OrderByDescending<TValue>(SqlValueExpression<TEntity, TValue> expression);

		IOrderByQueryBuilder<TEntity> OrderByDescending<TValue>(Expression<Func<TEntity, TValue>> expression);

		IOrderByQueryBuilder<TEntity> OrderByDescending<TValue>(SqlStorageField<TEntity, TValue> field);
	}
}
