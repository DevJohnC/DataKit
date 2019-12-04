using DataKit.ORM.Schema.Sql;
using DataKit.ORM.Sql.Expressions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DataKit.ORM.Sql.QueryBuilding
{
	public interface IRangeQueryBuilder<TEntity> : IQueryBuilder<TEntity>
		where TEntity : class
	{
		RangeBuilder<TEntity> RangeClause { get; }

		IRangeQueryBuilder<TEntity> Limit<TValue>(SqlValueExpression<TEntity, TValue> expression);

		IRangeQueryBuilder<TEntity> Limit(int limit);

		IRangeQueryBuilder<TEntity> Limit<TValue>(Expression<Func<TEntity, TValue>> expression);

		IRangeQueryBuilder<TEntity> Limit<TValue>(SqlStorageField<TEntity, TValue> field);

		IRangeQueryBuilder<TEntity> Offset<TValue>(SqlValueExpression<TEntity, TValue> expression);

		IRangeQueryBuilder<TEntity> Offset(int offset);

		IRangeQueryBuilder<TEntity> Offset<TValue>(Expression<Func<TEntity, TValue>> expression);

		IRangeQueryBuilder<TEntity> Offset<TValue>(SqlStorageField<TEntity, TValue> field);
	}
}
