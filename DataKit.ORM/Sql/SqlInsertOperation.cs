using DataKit.Mapping.Binding;
using DataKit.Modelling.TypeModels;
using DataKit.ORM.Sql.QueryBuilding;
using DataKit.SQL.Providers;
using DataKit.SQL.QueryExpressions;
using System;
using System.Linq.Expressions;

namespace DataKit.ORM.Sql
{
	public class SqlInsertOperation<TEntity> : SqlDataOperation<TEntity>, IAssignmentQueryBuilder<TEntity>
		where TEntity : class
	{
		public SqlInsertOperation(SqlDataSet<TEntity> dataSet, IQueryProvider queryProvider) : base(dataSet, queryProvider)
		{
			_queryBuilder = new SqlInsertBuilder<TEntity>(dataSet.DataModel);
		}

		private readonly SqlInsertBuilder<TEntity> _queryBuilder;
		FieldAssignmentBuilder<TEntity> IAssignmentQueryBuilder<TEntity>.Assignments => ((IAssignmentQueryBuilder<TEntity>)_queryBuilder).Assignments;

		public SqlInsertOperation<TEntity> Table(string tableName)
		{
			_queryBuilder.Table(tableName);
			return this;
		}

		IQueryBuilder<TEntity> IQueryBuilder<TEntity>.Table(string tableName)
			=> Table(tableName);

		public SqlInsertOperation<TEntity> Set<TProperty>(Expression<Func<TEntity, TProperty>> fieldSelector, TProperty value)
		{
			_queryBuilder.Set(fieldSelector, value);
			return this;
		}

		public SqlInsertOperation<TEntity> Set<TProperty>(Expression<Func<TEntity, TProperty>> fieldSelector, Expression<Func<TEntity, TProperty>> valueExpression)
		{
			_queryBuilder.Set(fieldSelector, valueExpression);
			return this;
		}

		public SqlInsertOperation<TEntity> Set(TEntity entity)
		{
			_queryBuilder.Set(entity);
			return this;
		}

		public SqlInsertOperation<TEntity> Set<TView>(
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
