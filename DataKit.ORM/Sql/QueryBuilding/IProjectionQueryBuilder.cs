using DataKit.ORM.Schema.Sql;
using DataKit.ORM.Sql.Mapping;
using System;
using System.Linq.Expressions;

namespace DataKit.ORM.Sql.QueryBuilding
{
	public interface IProjectionQueryBuilder<TEntity>
		where TEntity : class
	{
		ProjectionBuilder<TEntity> Projection { get; }

		IResultMapper<TView> Select<TView>(
			ProjectionModel<TEntity, TView> projectionModel
			) where TView : class;

		IResultMapper<T> Select<T>(SqlStorageField<TEntity, T> storageField);

		IResultMapper<T> Select<T>(Expression<Func<TEntity, T>> expression);
	}
}
