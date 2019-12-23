using DataKit.Mapping;
using DataKit.ORM.Schema.Sql;
using DataKit.ORM.Sql.Expressions;
using DataKit.SQL.QueryExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DataKit.ORM.Sql.QueryBuilding
{
	public class SqlSelectBuilder<TEntity> : SqlQueryBuilder<TEntity>,
		IProjectionQueryBuilder<TEntity>,
		IWhereQueryBuilder<TEntity>,
		IHavingQueryBuilder<TEntity>,
		IGroupByQueryBuilder<TEntity>,
		IOrderByQueryBuilder<TEntity>,
		IRangeQueryBuilder<TEntity>,
		IAliasIdentifier
		where TEntity : class
	{
		private string _tableName;

		string IAliasIdentifier.AliasIdentifier => _tableName;

		private ConditionBuilder<TEntity> _whereBuilder;
		ConditionBuilder<TEntity> IWhereQueryBuilder<TEntity>.WhereCondition => _whereBuilder;

		private ConditionBuilder<TEntity> _havingBuilder;
		ConditionBuilder<TEntity> IHavingQueryBuilder<TEntity>.HavingCondition => _havingBuilder;

		private GroupByBuilder<TEntity> _groupByBuilder;
		GroupByBuilder<TEntity> IGroupByQueryBuilder<TEntity>.GroupByClause => _groupByBuilder;

		private OrderByBuilder<TEntity> _orderByBuilder;
		OrderByBuilder<TEntity> IOrderByQueryBuilder<TEntity>.OrderByClause => _orderByBuilder;

		private RangeBuilder<TEntity> _rangeBuilder;
		RangeBuilder<TEntity> IRangeQueryBuilder<TEntity>.RangeClause => _rangeBuilder;

		private ProjectionBuilder<TEntity> _projectionBuilder;
		ProjectionBuilder<TEntity> IProjectionQueryBuilder<TEntity>.Projection => _projectionBuilder;

		public SqlSelectBuilder(SqlDataModel<TEntity> dataModel, IObjectFactory objectFactory = null) :
			base(dataModel)
		{
			_tableName = DataModel.StorageModel.DefaultTableName;
			_whereBuilder = new ConditionBuilder<TEntity>(dataModel, ConditionConverter);
			_havingBuilder = new ConditionBuilder<TEntity>(dataModel, ConditionConverter);
			_groupByBuilder = new GroupByBuilder<TEntity>(dataModel, ValueConverter);
			_rangeBuilder = new RangeBuilder<TEntity>(dataModel, ValueConverter);
			_projectionBuilder = new ProjectionBuilder<TEntity>(dataModel, ValueConverter, objectFactory);
			_orderByBuilder = new OrderByBuilder<TEntity>(dataModel, ValueConverter);
		}

		private IEnumerable<JoinBuilder<TEntity>> ConcatUniqueJoins(params JoinBuilder<TEntity>[][] joins)
		{
			IEnumerable<JoinBuilder<TEntity>> result = new JoinBuilder<TEntity>[0];
			foreach (var joinArray in joins)
			{
				if (joinArray == null || joinArray.Length < 1)
					continue;
				result = result.Concat(joinArray);
			}
			return result
				.GroupBy(join => join.Specification)
				.Select(joinGroup => joinGroup.First())
				.ToArray();
		}

		public override ExecutableQueryExpression BuildQuery()
		{
			var projectionExpressions = _projectionBuilder.ProjectionExpressions.ToArray();
			if (projectionExpressions.Length < 1)
				throw new InvalidOperationException("At least 1 projected field must be specified.");

			var where = _whereBuilder.Build();
			var having = _havingBuilder.Build();
			var limit = _rangeBuilder.Build();
			var groupBy = _groupByBuilder.Build();
			var orderBy = _orderByBuilder.Build();

			var groupByExpressions = groupBy?.Select(q => q.QueryExpression).OfType<GroupByQueryExpression>().ToArray();
			var groupByJoins = groupBy?.Where(q => q.RequiresJoins).SelectMany(q => q.Joins).ToArray();

			var orderByExpressions = orderBy?.Select(q => q.QueryExpression).OfType<OrderByQueryExpression>().ToArray();
			var orderByJoins = orderBy?.Where(q => q.RequiresJoins).SelectMany(q => q.Joins).ToArray();

			var joins = ConcatUniqueJoins(
				_projectionBuilder.Joins.ToArray(),
				where?.Joins,
				having?.Joins,
				limit?.Joins,
				groupByJoins,
				orderByJoins
				);
			var builtJoins = joins.Select(q => q.Build()).ToArray();

			return QueryExpression.Select(
				projection: projectionExpressions,
				from: QueryExpression.Table(_tableName),
				joins: builtJoins,
				where: where?.QueryExpression,
				having: having?.QueryExpression,
				limit: limit?.QueryExpression as LimitQueryExpression,
				orderBy: orderByExpressions,
				groupBy: groupByExpressions
				);
		}

		public SqlSelectBuilder<TEntity> Table(string tableName)
		{
			_tableName = tableName;
			return this;
		}

		IQueryBuilder<TEntity> IQueryBuilder<TEntity>.Table(string tableName)
			=> Table(tableName);

		public SqlSelectBuilder<TEntity> AndWhere(SqlValueExpression<TEntity, bool> conditionExpression)
		{
			_whereBuilder.AndAlso(conditionExpression);
			return this;
		}

		public SqlSelectBuilder<TEntity> AndWhere(Expression<Func<TEntity, bool>> conditionExpression)
		{
			_whereBuilder.AndAlso(conditionExpression);
			return this;
		}

		public SqlSelectBuilder<TEntity> OrWhere(SqlValueExpression<TEntity, bool> conditionExpression)
		{
			_whereBuilder.OrElse(conditionExpression);
			return this;
		}

		public SqlSelectBuilder<TEntity> OrWhere(Expression<Func<TEntity, bool>> conditionExpression)
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

		public SqlSelectBuilder<TEntity> AndWhere<TValue>(SqlStorageField<TEntity, TValue> field, SqlComparisonOperator comparisonType, TValue value)
		{
			_whereBuilder.AndAlso(field, comparisonType, value);
			return this;
		}

		public SqlSelectBuilder<TEntity> OrWhere<TValue>(SqlStorageField<TEntity, TValue> field, SqlComparisonOperator comparisonType, TValue value)
		{
			_whereBuilder.OrElse(field, comparisonType, value);
			return this;
		}

		IWhereQueryBuilder<TEntity> IWhereQueryBuilder<TEntity>.AndWhere<TValue>(SqlStorageField<TEntity, TValue> field, SqlComparisonOperator comparisonType, TValue value)
			=> AndWhere(field, comparisonType, value);

		IWhereQueryBuilder<TEntity> IWhereQueryBuilder<TEntity>.OrWhere<TValue>(SqlStorageField<TEntity, TValue> field, SqlComparisonOperator comparisonType, TValue value)
			=> OrWhere(field, comparisonType, value);

		IHavingQueryBuilder<TEntity> IHavingQueryBuilder<TEntity>.AndHaving(SqlValueExpression<TEntity, bool> conditionExpression)
			=> AndHaving(conditionExpression);

		IHavingQueryBuilder<TEntity> IHavingQueryBuilder<TEntity>.AndHaving(Expression<Func<TEntity, bool>> conditionExpression)
			=> AndHaving(conditionExpression);

		IHavingQueryBuilder<TEntity> IHavingQueryBuilder<TEntity>.AndHaving<TValue>(SqlStorageField<TEntity, TValue> field, SqlComparisonOperator comparisonType, TValue value)
			=> AndHaving(field, comparisonType, value);

		IHavingQueryBuilder<TEntity> IHavingQueryBuilder<TEntity>.OrHaving(SqlValueExpression<TEntity, bool> conditionExpression)
			=> OrHaving(conditionExpression);

		IHavingQueryBuilder<TEntity> IHavingQueryBuilder<TEntity>.OrHaving(Expression<Func<TEntity, bool>> conditionExpression)
			=> OrHaving(conditionExpression);

		IHavingQueryBuilder<TEntity> IHavingQueryBuilder<TEntity>.OrHaving<TValue>(SqlStorageField<TEntity, TValue> field, SqlComparisonOperator comparisonType, TValue value)
			=> OrHaving(field, comparisonType, value);

		public SqlSelectBuilder<TEntity> AndHaving(SqlValueExpression<TEntity, bool> conditionExpression)
		{
			_havingBuilder.AndAlso(conditionExpression);
			return this;
		}

		public SqlSelectBuilder<TEntity> AndHaving(Expression<Func<TEntity, bool>> conditionExpression)
		{
			_havingBuilder.AndAlso(conditionExpression);
			return this;
		}

		public SqlSelectBuilder<TEntity> AndHaving<TValue>(SqlStorageField<TEntity, TValue> field, SqlComparisonOperator comparisonType, TValue value)
		{
			_havingBuilder.AndAlso(field, comparisonType, value);
			return this;
		}

		public SqlSelectBuilder<TEntity> OrHaving(SqlValueExpression<TEntity, bool> conditionExpression)
		{
			_havingBuilder.OrElse(conditionExpression);
			return this;
		}

		public SqlSelectBuilder<TEntity> OrHaving(Expression<Func<TEntity, bool>> conditionExpression)
		{
			_havingBuilder.OrElse(conditionExpression);
			return this;
		}

		public SqlSelectBuilder<TEntity> OrHaving<TValue>(SqlStorageField<TEntity, TValue> field, SqlComparisonOperator comparisonType, TValue value)
		{
			_havingBuilder.OrElse(field, comparisonType, value);
			return this;
		}

		IGroupByQueryBuilder<TEntity> IGroupByQueryBuilder<TEntity>.GroupBy<TValue>(SqlValueExpression<TEntity, TValue> expression)
			=> GroupBy(expression);

		IGroupByQueryBuilder<TEntity> IGroupByQueryBuilder<TEntity>.GroupBy<TValue>(Expression<Func<TEntity, TValue>> expression)
			=> GroupBy(expression);

		IGroupByQueryBuilder<TEntity> IGroupByQueryBuilder<TEntity>.GroupBy<TValue>(SqlStorageField<TEntity, TValue> field)
			=> GroupBy(field);

		public SqlSelectBuilder<TEntity> GroupBy<TValue>(SqlValueExpression<TEntity, TValue> expression)
		{
			_groupByBuilder.GroupBy(expression);
			return this;
		}

		public SqlSelectBuilder<TEntity> GroupBy<TValue>(Expression<Func<TEntity, TValue>> expression)
		{
			_groupByBuilder.GroupBy(expression);
			return this;
		}

		public SqlSelectBuilder<TEntity> GroupBy<TValue>(SqlStorageField<TEntity, TValue> field)
		{
			_groupByBuilder.GroupBy(field);
			return this;
		}

		IOrderByQueryBuilder<TEntity> IOrderByQueryBuilder<TEntity>.OrderBy<TValue>(SqlValueExpression<TEntity, TValue> expression)
			=> OrderBy(expression);

		IOrderByQueryBuilder<TEntity> IOrderByQueryBuilder<TEntity>.OrderBy<TValue>(Expression<Func<TEntity, TValue>> expression)
			=> OrderBy(expression);

		IOrderByQueryBuilder<TEntity> IOrderByQueryBuilder<TEntity>.OrderBy<TValue>(SqlStorageField<TEntity, TValue> field)
			=> OrderBy(field);

		IOrderByQueryBuilder<TEntity> IOrderByQueryBuilder<TEntity>.OrderByDescending<TValue>(SqlValueExpression<TEntity, TValue> expression)
			=> OrderByDescending(expression);

		IOrderByQueryBuilder<TEntity> IOrderByQueryBuilder<TEntity>.OrderByDescending<TValue>(Expression<Func<TEntity, TValue>> expression)
			=> OrderByDescending(expression);

		IOrderByQueryBuilder<TEntity> IOrderByQueryBuilder<TEntity>.OrderByDescending<TValue>(SqlStorageField<TEntity, TValue> field)
			=> OrderByDescending(field);

		public SqlSelectBuilder<TEntity> OrderBy<TValue>(SqlValueExpression<TEntity, TValue> expression)
		{
			_orderByBuilder.OrderBy(expression);
			return this;
		}

		public SqlSelectBuilder<TEntity> OrderBy<TValue>(Expression<Func<TEntity, TValue>> expression)
		{
			_orderByBuilder.OrderBy(expression);
			return this;
		}

		public SqlSelectBuilder<TEntity> OrderBy<TValue>(SqlStorageField<TEntity, TValue> field)
		{
			_orderByBuilder.OrderBy(field);
			return this;
		}

		public SqlSelectBuilder<TEntity> OrderByDescending<TValue>(SqlValueExpression<TEntity, TValue> expression)
		{
			_orderByBuilder.OrderByDescending(expression);
			return this;
		}

		public SqlSelectBuilder<TEntity> OrderByDescending<TValue>(Expression<Func<TEntity, TValue>> expression)
		{
			_orderByBuilder.OrderByDescending(expression);
			return this;
		}

		public SqlSelectBuilder<TEntity> OrderByDescending<TValue>(SqlStorageField<TEntity, TValue> field)
		{
			_orderByBuilder.OrderByDescending(field);
			return this;
		}

		IRangeQueryBuilder<TEntity> IRangeQueryBuilder<TEntity>.Limit<TValue>(SqlValueExpression<TEntity, TValue> expression)
			=> Limit(expression);

		IRangeQueryBuilder<TEntity> IRangeQueryBuilder<TEntity>.Limit<TValue>(Expression<Func<TEntity, TValue>> expression)
			=> Limit(expression);

		IRangeQueryBuilder<TEntity> IRangeQueryBuilder<TEntity>.Limit<TValue>(SqlStorageField<TEntity, TValue> field)
			=> Limit(field);

		IRangeQueryBuilder<TEntity> IRangeQueryBuilder<TEntity>.Offset<TValue>(SqlValueExpression<TEntity, TValue> expression)
			=> Offset(expression);

		IRangeQueryBuilder<TEntity> IRangeQueryBuilder<TEntity>.Offset<TValue>(Expression<Func<TEntity, TValue>> expression)
			=> Offset(expression);

		IRangeQueryBuilder<TEntity> IRangeQueryBuilder<TEntity>.Offset<TValue>(SqlStorageField<TEntity, TValue> field)
			=> Offset(field);

		public SqlSelectBuilder<TEntity> Limit<TValue>(SqlValueExpression<TEntity, TValue> expression)
		{
			_rangeBuilder.Limit(expression);
			return this;
		}

		public SqlSelectBuilder<TEntity> Limit<TValue>(Expression<Func<TEntity, TValue>> expression)
		{
			_rangeBuilder.Limit(expression);
			return this;
		}

		public SqlSelectBuilder<TEntity> Limit<TValue>(SqlStorageField<TEntity, TValue> field)
		{
			_rangeBuilder.Limit(field);
			return this;
		}

		public SqlSelectBuilder<TEntity> Offset<TValue>(SqlValueExpression<TEntity, TValue> expression)
		{
			_rangeBuilder.Offset(expression);
			return this;
		}

		public SqlSelectBuilder<TEntity> Offset<TValue>(Expression<Func<TEntity, TValue>> expression)
		{
			_rangeBuilder.Offset(expression);
			return this;
		}

		public SqlSelectBuilder<TEntity> Offset<TValue>(SqlStorageField<TEntity, TValue> field)
		{
			_rangeBuilder.Offset(field);
			return this;
		}

		public SqlSelectBuilder<TEntity> Limit(int limit)
		{
			_rangeBuilder.Limit(limit);
			return this;
		}

		public SqlSelectBuilder<TEntity> Offset(int offset)
		{
			_rangeBuilder.Offset(offset);
			return this;
		}

		IRangeQueryBuilder<TEntity> IRangeQueryBuilder<TEntity>.Limit(int limit)
			=> Limit(limit);

		IRangeQueryBuilder<TEntity> IRangeQueryBuilder<TEntity>.Offset(int offset)
			=> Offset(offset);

		public Mapping.IResultMapper<TView> Select<TView>(ProjectionModel<TEntity, TView> projectionModel) where TView : class
			=> _projectionBuilder.Select(projectionModel);

		public Mapping.IResultMapper<T> Select<T>(SqlStorageField<TEntity, T> storageField)
			=> _projectionBuilder.Select(storageField);

		public Mapping.IResultMapper<T> Select<T>(Expression<Func<TEntity, T>> expression)
			=> _projectionBuilder.Select(expression);
	}
}
