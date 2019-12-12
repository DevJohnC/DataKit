using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DataKit.Mapping;
using DataKit.Mapping.Binding;
using DataKit.Modelling.TypeModels;
using DataKit.ORM.Sql.Expressions;
using DataKit.ORM.Sql.Mapping;
using DataKit.ORM.Sql.QueryBuilding;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.Queries;
using IQueryProvider = Silk.Data.SQL.Providers.IQueryProvider;

namespace DataKit.ORM.Sql
{
	public abstract class SqlSelectOperationBase<TEntity, TSelf> : SqlDataOperationBase<TEntity>,
		IWhereQueryBuilder<TEntity>,
		IHavingQueryBuilder<TEntity>,
		IOrderByQueryBuilder<TEntity>,
		IGroupByQueryBuilder<TEntity>,
		IRangeQueryBuilder<TEntity>
		where TEntity : class
		where TSelf : SqlSelectOperationBase<TEntity, TSelf>
	{
		private readonly TSelf _self;

		internal readonly SqlSelectBuilder<TEntity> QueryBuilder;

		protected SqlSelectOperationBase(SqlDataSet<TEntity> dataSet, IQueryProvider queryProvider, IObjectFactory objectFactory) :
			base(dataSet, queryProvider)
		{
			QueryBuilder = new SqlSelectBuilder<TEntity>(dataSet.DataModel, objectFactory);
			_self = (TSelf)this;
		}

		protected SqlSelectOperationBase(SqlDataSet<TEntity> dataSet, IQueryProvider queryProvider, SqlSelectBuilder<TEntity> queryBuilder) :
			base(dataSet, queryProvider)
		{
			QueryBuilder = queryBuilder;
			_self = (TSelf)this;
		}

		protected override QueryExpression BuildQuery()
			=> QueryBuilder.BuildQuery();

		ConditionBuilder<TEntity> IWhereQueryBuilder<TEntity>.WhereCondition => ((IWhereQueryBuilder<TEntity>)QueryBuilder).WhereCondition;

		ConditionBuilder<TEntity> IHavingQueryBuilder<TEntity>.HavingCondition => ((IHavingQueryBuilder<TEntity>)QueryBuilder).HavingCondition;

		OrderByBuilder<TEntity> IOrderByQueryBuilder<TEntity>.OrderByClause => ((IOrderByQueryBuilder<TEntity>)QueryBuilder).OrderByClause;

		GroupByBuilder<TEntity> IGroupByQueryBuilder<TEntity>.GroupByClause => ((IGroupByQueryBuilder<TEntity>)QueryBuilder).GroupByClause;

		RangeBuilder<TEntity> IRangeQueryBuilder<TEntity>.RangeClause => ((IRangeQueryBuilder<TEntity>)QueryBuilder).RangeClause;

		public TSelf AndHaving(SqlValueExpression<TEntity, bool> conditionExpression)
		{
			QueryBuilder.AndHaving(conditionExpression);
			return _self;
		}

		public TSelf AndHaving(Expression<Func<TEntity, bool>> conditionExpression)
		{
			QueryBuilder.AndHaving(conditionExpression);
			return _self;
		}

		public TSelf AndHaving<TValue>(Schema.Sql.SqlStorageField<TEntity, TValue> field, ComparisonOperator comparisonType, TValue value)
		{
			QueryBuilder.AndHaving(field, comparisonType, value);
			return _self;
		}

		public TSelf AndWhere(SqlValueExpression<TEntity, bool> conditionExpression)
		{
			QueryBuilder.AndWhere(conditionExpression);
			return _self;
		}

		public TSelf AndWhere(Expression<Func<TEntity, bool>> conditionExpression)
		{
			QueryBuilder.AndWhere(conditionExpression);
			return _self;
		}

		public TSelf AndWhere<TValue>(Schema.Sql.SqlStorageField<TEntity, TValue> field, ComparisonOperator comparisonType, TValue value)
		{
			QueryBuilder.AndWhere(field, comparisonType, value);
			return _self;
		}

		public TSelf GroupBy<TValue>(SqlValueExpression<TEntity, TValue> expression)
		{
			QueryBuilder.GroupBy(expression);
			return _self;
		}

		public TSelf GroupBy<TValue>(Expression<Func<TEntity, TValue>> expression)
		{
			QueryBuilder.GroupBy(expression);
			return _self;
		}

		public TSelf GroupBy<TValue>(Schema.Sql.SqlStorageField<TEntity, TValue> field)
		{
			QueryBuilder.GroupBy(field);
			return _self;
		}

		public TSelf Limit<TValue>(SqlValueExpression<TEntity, TValue> expression)
		{
			QueryBuilder.Limit(expression);
			return _self;
		}

		public TSelf Limit(int limit)
		{
			QueryBuilder.Limit(limit);
			return _self;
		}

		public TSelf Limit<TValue>(Expression<Func<TEntity, TValue>> expression)
		{
			QueryBuilder.Limit(expression);
			return _self;
		}

		public TSelf Limit<TValue>(Schema.Sql.SqlStorageField<TEntity, TValue> field)
		{
			QueryBuilder.Limit(field);
			return _self;
		}

		public TSelf Offset<TValue>(SqlValueExpression<TEntity, TValue> expression)
		{
			QueryBuilder.Offset(expression);
			return _self;
		}

		public TSelf Offset(int offset)
		{
			QueryBuilder.Offset(offset);
			return _self;
		}

		public TSelf Offset<TValue>(Expression<Func<TEntity, TValue>> expression)
		{
			QueryBuilder.Offset(expression);
			return _self;
		}

		public TSelf Offset<TValue>(Schema.Sql.SqlStorageField<TEntity, TValue> field)
		{
			QueryBuilder.Offset(field);
			return _self;
		}

		public TSelf OrderBy<TValue>(SqlValueExpression<TEntity, TValue> expression)
		{
			QueryBuilder.OrderBy(expression);
			return _self;
		}

		public TSelf OrderBy<TValue>(Expression<Func<TEntity, TValue>> expression)
		{
			QueryBuilder.OrderBy(expression);
			return _self;
		}

		public TSelf OrderBy<TValue>(Schema.Sql.SqlStorageField<TEntity, TValue> field)
		{
			QueryBuilder.OrderBy(field);
			return _self;
		}

		public TSelf OrderByDescending<TValue>(SqlValueExpression<TEntity, TValue> expression)
		{
			QueryBuilder.OrderByDescending(expression);
			return _self;
		}

		public TSelf OrderByDescending<TValue>(Expression<Func<TEntity, TValue>> expression)
		{
			QueryBuilder.OrderByDescending(expression);
			return _self;
		}

		public TSelf OrderByDescending<TValue>(Schema.Sql.SqlStorageField<TEntity, TValue> field)
		{
			QueryBuilder.OrderByDescending(field);
			return _self;
		}

		public TSelf OrHaving(SqlValueExpression<TEntity, bool> conditionExpression)
		{
			QueryBuilder.OrHaving(conditionExpression);
			return _self;
		}

		public TSelf OrHaving(Expression<Func<TEntity, bool>> conditionExpression)
		{
			QueryBuilder.OrHaving(conditionExpression);
			return _self;
		}

		public TSelf OrHaving<TValue>(Schema.Sql.SqlStorageField<TEntity, TValue> field, ComparisonOperator comparisonType, TValue value)
		{
			QueryBuilder.OrHaving(field, comparisonType, value);
			return _self;
		}

		public TSelf OrWhere(SqlValueExpression<TEntity, bool> conditionExpression)
		{
			QueryBuilder.OrWhere(conditionExpression);
			return _self;
		}

		public TSelf OrWhere(Expression<Func<TEntity, bool>> conditionExpression)
		{
			QueryBuilder.OrWhere(conditionExpression);
			return _self;
		}

		public TSelf OrWhere<TValue>(Schema.Sql.SqlStorageField<TEntity, TValue> field, ComparisonOperator comparisonType, TValue value)
		{
			QueryBuilder.OrWhere(field, comparisonType, value);
			return _self;
		}

		public TSelf Table(string tableName)
		{
			QueryBuilder.Table(tableName);
			return _self;
		}

		IHavingQueryBuilder<TEntity> IHavingQueryBuilder<TEntity>.AndHaving(SqlValueExpression<TEntity, bool> conditionExpression)
			=> AndHaving(conditionExpression);

		IHavingQueryBuilder<TEntity> IHavingQueryBuilder<TEntity>.AndHaving(Expression<Func<TEntity, bool>> conditionExpression)
			=> AndHaving(conditionExpression);

		IHavingQueryBuilder<TEntity> IHavingQueryBuilder<TEntity>.AndHaving<TValue>(Schema.Sql.SqlStorageField<TEntity, TValue> field, ComparisonOperator comparisonType, TValue value)
			=> AndHaving(field, comparisonType, value);

		IWhereQueryBuilder<TEntity> IWhereQueryBuilder<TEntity>.AndWhere(SqlValueExpression<TEntity, bool> conditionExpression)
			=> AndWhere(conditionExpression);

		IWhereQueryBuilder<TEntity> IWhereQueryBuilder<TEntity>.AndWhere(Expression<Func<TEntity, bool>> conditionExpression)
			=> AndWhere(conditionExpression);

		IWhereQueryBuilder<TEntity> IWhereQueryBuilder<TEntity>.AndWhere<TValue>(Schema.Sql.SqlStorageField<TEntity, TValue> field, ComparisonOperator comparisonType, TValue value)
			=> AndWhere(field, comparisonType, value);

		IGroupByQueryBuilder<TEntity> IGroupByQueryBuilder<TEntity>.GroupBy<TValue>(SqlValueExpression<TEntity, TValue> expression)
			=> GroupBy(expression);

		IGroupByQueryBuilder<TEntity> IGroupByQueryBuilder<TEntity>.GroupBy<TValue>(Expression<Func<TEntity, TValue>> expression)
			=> GroupBy(expression);

		IGroupByQueryBuilder<TEntity> IGroupByQueryBuilder<TEntity>.GroupBy<TValue>(Schema.Sql.SqlStorageField<TEntity, TValue> field)
			=> GroupBy(field);

		IRangeQueryBuilder<TEntity> IRangeQueryBuilder<TEntity>.Limit<TValue>(SqlValueExpression<TEntity, TValue> expression)
			=> Limit(expression);

		IRangeQueryBuilder<TEntity> IRangeQueryBuilder<TEntity>.Limit(int limit)
			=> Limit(limit);

		IRangeQueryBuilder<TEntity> IRangeQueryBuilder<TEntity>.Limit<TValue>(Expression<Func<TEntity, TValue>> expression)
			=> Limit(expression);

		IRangeQueryBuilder<TEntity> IRangeQueryBuilder<TEntity>.Limit<TValue>(Schema.Sql.SqlStorageField<TEntity, TValue> field)
			=> Limit(field);

		IRangeQueryBuilder<TEntity> IRangeQueryBuilder<TEntity>.Offset<TValue>(SqlValueExpression<TEntity, TValue> expression)
			=> Offset(expression);

		IRangeQueryBuilder<TEntity> IRangeQueryBuilder<TEntity>.Offset(int offset)
			=> Offset(offset);

		IRangeQueryBuilder<TEntity> IRangeQueryBuilder<TEntity>.Offset<TValue>(Expression<Func<TEntity, TValue>> expression)
			=> Offset(expression);

		IRangeQueryBuilder<TEntity> IRangeQueryBuilder<TEntity>.Offset<TValue>(Schema.Sql.SqlStorageField<TEntity, TValue> field)
			=> Offset(field);

		IOrderByQueryBuilder<TEntity> IOrderByQueryBuilder<TEntity>.OrderBy<TValue>(SqlValueExpression<TEntity, TValue> expression)
			=> OrderBy(expression);

		IOrderByQueryBuilder<TEntity> IOrderByQueryBuilder<TEntity>.OrderBy<TValue>(Expression<Func<TEntity, TValue>> expression)
			=> OrderBy(expression);

		IOrderByQueryBuilder<TEntity> IOrderByQueryBuilder<TEntity>.OrderBy<TValue>(Schema.Sql.SqlStorageField<TEntity, TValue> field)
			=> OrderBy(field);

		IOrderByQueryBuilder<TEntity> IOrderByQueryBuilder<TEntity>.OrderByDescending<TValue>(SqlValueExpression<TEntity, TValue> expression)
			=> OrderByDescending(expression);

		IOrderByQueryBuilder<TEntity> IOrderByQueryBuilder<TEntity>.OrderByDescending<TValue>(Expression<Func<TEntity, TValue>> expression)
			=> OrderByDescending(expression);

		IOrderByQueryBuilder<TEntity> IOrderByQueryBuilder<TEntity>.OrderByDescending<TValue>(Schema.Sql.SqlStorageField<TEntity, TValue> field)
			=> OrderByDescending(field);

		IHavingQueryBuilder<TEntity> IHavingQueryBuilder<TEntity>.OrHaving(SqlValueExpression<TEntity, bool> conditionExpression)
			=> OrHaving(conditionExpression);

		IHavingQueryBuilder<TEntity> IHavingQueryBuilder<TEntity>.OrHaving(Expression<Func<TEntity, bool>> conditionExpression)
			=> OrHaving(conditionExpression);

		IHavingQueryBuilder<TEntity> IHavingQueryBuilder<TEntity>.OrHaving<TValue>(Schema.Sql.SqlStorageField<TEntity, TValue> field, ComparisonOperator comparisonType, TValue value)
			=> OrHaving(field, comparisonType, value);

		IWhereQueryBuilder<TEntity> IWhereQueryBuilder<TEntity>.OrWhere(SqlValueExpression<TEntity, bool> conditionExpression)
			=> OrWhere(conditionExpression);

		IWhereQueryBuilder<TEntity> IWhereQueryBuilder<TEntity>.OrWhere(Expression<Func<TEntity, bool>> conditionExpression)
			=> OrWhere(conditionExpression);

		IWhereQueryBuilder<TEntity> IWhereQueryBuilder<TEntity>.OrWhere<TValue>(Schema.Sql.SqlStorageField<TEntity, TValue> field, ComparisonOperator comparisonType, TValue value)
			=> OrWhere(field, comparisonType, value);

		IQueryBuilder<TEntity> IQueryBuilder<TEntity>.Table(string tableName)
			=> Table(tableName);

		protected QueryResultReader<TResult> ExecuteQuery<TResult>(Func<QueryResult, TResult> mappingCallback,
			Action<QueryResult, TResult> injectingCallback)
		{
			var queryResult = QueryProvider.ExecuteReader(BuildQuery());
			return new QueryResultReader<TResult>(queryResult, mappingCallback, injectingCallback);
		}

		protected async Task<QueryResultReader<TResult>> ExecuteQueryAsync<TResult>(Func<QueryResult, TResult> mappingCallback,
			Action<QueryResult, TResult> injectingCallback)
		{
			var queryResult = await QueryProvider.ExecuteReaderAsync(BuildQuery());
			return new QueryResultReader<TResult>(queryResult, mappingCallback, injectingCallback);
		}

		protected class QueryResultReader<T> : IDisposable
		{
			private readonly QueryResult _queryResult;
			private readonly Func<QueryResult, T> _mapSingleResult;
			private readonly Action<QueryResult, T> _injectingCallback;

			public bool HasRows => _queryResult.HasRows;

			public QueryResultReader(QueryResult queryResult, Func<QueryResult, T> mapSingleResult,
				Action<QueryResult, T> injectingCallback)
			{
				_queryResult = queryResult;
				_mapSingleResult = mapSingleResult;
				_injectingCallback = injectingCallback;
			}

			public (bool Success, T Result) ReadNext()
			{
				if (!_queryResult.Read())
					return (false, default);

				return (true, _mapSingleResult(_queryResult));
			}

			public bool InjectNext(T instance)
			{
				if (!_queryResult.Read())
					return false;
				_injectingCallback(_queryResult, instance);
				return true;
			}

			public async Task<(bool Success, T Result)> ReadNextAsync()
			{
				if (!await _queryResult.ReadAsync())
					return (false, default);

				return (true, _mapSingleResult(_queryResult));
			}

			public async Task<bool> InjectNextAsync(T instance)
			{
				if (!await _queryResult.ReadAsync())
					return false;
				_injectingCallback(_queryResult, instance);
				return true;
			}

			public void Dispose()
			{
				_queryResult.Dispose();
			}
		}
	}

	public abstract class SqlSelectOperationBase<TEntity, TResult, TSelf> : SqlSelectOperationBase<TEntity, TSelf>,
		IProjectionQuery<TResult>, ISqlBatchableOperation<TResult>
		where TEntity : class
		where TSelf : SqlSelectOperationBase<TEntity, TResult, TSelf>
	{
		public SqlSelectOperationBase(SqlDataSet<TEntity> dataSet, IQueryProvider queryProvider, IObjectFactory objectFactory) :
			base(dataSet, queryProvider, objectFactory)
		{
		}

		public SqlSelectOperationBase(SqlDataSet<TEntity> dataSet, IQueryProvider queryProvider, SqlSelectBuilder<TEntity> queryBuilder) :
			base(dataSet, queryProvider, queryBuilder)
		{
		}

		public IReadOnlyList<TResult> ToList()
		{
			return ToEnumerable().ToList();
		}

		public async Task<IReadOnlyList<TResult>> ToListAsync(CancellationToken cancellationToken = default)
		{
#if NETSTANDARD2_1
			return await ToEnumerableAsync(cancellationToken)
				.ToListAsync(cancellationToken);
#else
			var result = new List<TResult>();
			using (var reader = await ExecuteQueryAsync(MapSingle, InjectSingle))
			{
				if (!reader.HasRows)
					return result;
				while (!cancellationToken.IsCancellationRequested)
				{
					var (success, record) = await reader.ReadNextAsync();
					if (!success)
						return result;
					result.Add(record);
				}
			}
			return result;
#endif
		}

		public void Inject<TInstance>(TInstance instance)
			where TInstance : class, TResult
		{
			Limit(1);
			using (var reader = ExecuteQuery(MapSingle, InjectSingle))
			{
				if (!reader.HasRows)
					return;
				reader.InjectNext(instance);
			}
		}

		public async Task InjectAsync<TInstance>(TInstance instance)
			where TInstance : class, TResult
		{
			Limit(1);
			using (var reader = await ExecuteQueryAsync(MapSingle, InjectSingle))
			{
				if (!reader.HasRows)
					return;
				await reader.InjectNextAsync(instance);
			}
		}

		public TResult ToSingle()
		{
			Limit(1);
			foreach (var record in ToEnumerable())
				return record;
			return default;
		}

		public async Task<TResult> ToSingleAsync(CancellationToken cancellationToken = default)
		{
			Limit(1);
			using (var reader = await ExecuteQueryAsync(MapSingle, InjectSingle))
			{
				if (!reader.HasRows)
					return default;
				var (success, record) = await reader.ReadNextAsync();
				return record;
			}
		}

		public IEnumerable<TResult> ToEnumerable()
		{
			using (var reader = ExecuteQuery(MapSingle, InjectSingle))
			{
				if (!reader.HasRows)
					yield break;
				while (true)
				{
					var (success, record) = reader.ReadNext();
					if (!success)
						yield break;
					yield return record;
				}
			}
		}

#if NETSTANDARD2_1
		public async IAsyncEnumerable<TResult> ToEnumerableAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			using (var reader = await ExecuteQueryAsync(MapSingle, InjectSingle))
			{
				if (!reader.HasRows)
					yield break;
				while (!cancellationToken.IsCancellationRequested)
				{
					var (success, record) = await reader.ReadNextAsync();
					if (!success)
						yield break;
					yield return record;
				}
			}
		}
#endif

		protected abstract TResult MapSingle(QueryResult queryResult);

		protected abstract void InjectSingle(QueryResult queryResult, TResult instance);

		QueryExpression ISqlBatchableOperation<TResult>.BuildQuery()
			=> BuildQuery();

		ISqlBatchProcessor<TResult> ISqlBatchableOperation<TResult>.GetSingleBatchProcessor()
		{
			Limit(1);
			return new SingleDeferredProcessor(MapSingle);
		}

		ISqlBatchProcessor<IReadOnlyList<TResult>> ISqlBatchableOperation<TResult>.GetListBatchProcessor()
		{
			return new ListDeferredProcessor(MapSingle);
		}

		ISqlBatchProcessor ISqlBatchableOperation<TResult>.GetInjectBatchProcessor(TResult instance)
		{
			Limit(1);
			return new InjectDeferredProcessor(InjectSingle, instance);
		}

		QueryExpression IProjectionQuery.BuildQuery()
			=> BuildQuery();

		private class InjectDeferredProcessor : ISqlBatchProcessor
		{
			private readonly Action<QueryResult, TResult> _injectingCallback;
			private readonly TResult _instance;

			public InjectDeferredProcessor(Action<QueryResult, TResult> injectingCallback, TResult instance)
			{
				_injectingCallback = injectingCallback;
				_instance = instance;
			}

			public void ProcessResult(QueryResult queryResult)
			{
				if (!queryResult.HasRows || !queryResult.Read())
					return;
				_injectingCallback(queryResult, _instance);
			}

			public async Task ProcessResultAsync(QueryResult queryResult)
			{
				if (!queryResult.HasRows || !await queryResult.ReadAsync())
					return;
				_injectingCallback(queryResult, _instance);
			}
		}

		private class SingleDeferredProcessor : ISqlBatchProcessor<TResult>
		{
			private readonly DeferredResultSource<TResult> _resultSource =
				new DeferredResultSource<TResult>();
			private readonly Func<QueryResult, TResult> _mappingCallback;

			public DeferredResult<TResult> Result => _resultSource.Result;

			public SingleDeferredProcessor(Func<QueryResult, TResult> mappingCallback)
			{
				_mappingCallback = mappingCallback;
			}

			public void ProcessResult(QueryResult queryResult)
			{
				if (!queryResult.HasRows || !queryResult.Read())
					return;
				_resultSource.SetResult(_mappingCallback(queryResult));
			}

			public async Task ProcessResultAsync(QueryResult queryResult)
			{
				if (!queryResult.HasRows || !await queryResult.ReadAsync())
					return;
				_resultSource.SetResult(_mappingCallback(queryResult));
			}
		}

		private class ListDeferredProcessor : ISqlBatchProcessor<IReadOnlyList<TResult>>
		{
			private readonly DeferredResultSource<IReadOnlyList<TResult>> _resultSource =
				new DeferredResultSource<IReadOnlyList<TResult>>();
			private readonly Func<QueryResult, TResult> _mappingCallback;

			public DeferredResult<IReadOnlyList<TResult>> Result => _resultSource.Result;

			public ListDeferredProcessor(Func<QueryResult, TResult> mappingCallback)
			{
				_mappingCallback = mappingCallback;
			}

			public void ProcessResult(QueryResult queryResult)
			{
				var result = new List<TResult>();
				_resultSource.SetResult(result);

				if (!queryResult.HasRows)
					return;

				while (queryResult.Read())
				{
					result.Add(_mappingCallback(queryResult));
				}
			}

			public async Task ProcessResultAsync(QueryResult queryResult)
			{
				var result = new List<TResult>();
				_resultSource.SetResult(result);

				if (!queryResult.HasRows)
					return;

				while (await queryResult.ReadAsync())
				{
					result.Add(_mappingCallback(queryResult));
				}
			}
		}
	}

	public class SqlSelectOperation<TEntity, TResult> : SqlSelectOperationBase<TEntity, TResult, SqlSelectOperation<TEntity, TResult>>
		where TEntity : class
	{
		private IResultMapper<TResult> _resultMapper;

		public SqlSelectOperation(SqlDataSet<TEntity> dataSet, IQueryProvider queryProvider, IObjectFactory objectFactory = null) :
			base(dataSet, queryProvider, objectFactory)
		{
		}

		public SqlSelectOperation<TEntity, TResult> Bind(Expression<Func<TEntity, TResult>> expression)
		{
			((IProjectionQueryBuilder<TEntity>)QueryBuilder).Projection.Clear();
			_resultMapper = QueryBuilder.Select(expression);
			return this;
		}

		public SqlSelectOperation<TEntity, TResult> Bind<T>(
			DataModelBinding<TypeModel<TEntity>, PropertyField, TypeModel<T>, PropertyField> binding = null
			)
			where T : class, TResult
		{
			((IProjectionQueryBuilder<TEntity>)QueryBuilder).Projection.Clear();
			_resultMapper = (IResultMapper<TResult>)QueryBuilder.Select(
				DataSet.Bindings.GetProjectionModel(binding)
				);
			return this;
		}

		public SqlSelectOperation<TEntity, TResult, TEntity> Select()
		{
			return new SqlSelectOperation<TEntity, TResult, TEntity>(DataSet, QueryProvider,
				QueryBuilder,
				_resultMapper,
				QueryBuilder.Select(DataSet.Bindings.GetProjectionModel<TEntity>()));
		}

		public SqlSelectOperation<TEntity, TResult, TView> Select<TView>(
			DataModelBinding<TypeModel<TEntity>, PropertyField, TypeModel<TView>, PropertyField> binding = null
			)
			where TView : class
		{
			return new SqlSelectOperation<TEntity, TResult, TView>(DataSet, QueryProvider,
				QueryBuilder,
				_resultMapper,
				QueryBuilder.Select(DataSet.Bindings.GetProjectionModel(binding)));
		}

		public SqlSelectOperation<TEntity, TResult, TExpr> Select<TExpr>(Expression<Func<TEntity, TExpr>> expression)
		{
			return new SqlSelectOperation<TEntity, TResult, TExpr>(DataSet, QueryProvider,
				QueryBuilder,
				_resultMapper,
				QueryBuilder.Select(expression));
		}

		protected override void InjectSingle(QueryResult queryResult, TResult instance)
			=> _resultMapper.InjectSingle(queryResult, instance);

		protected override TResult MapSingle(QueryResult queryResult)
			=> _resultMapper.MapSingle(queryResult);
	}

	public class SqlSelectOperation<TEntity, TResult1, TResult2> : SqlSelectOperationBase<TEntity, (TResult1, TResult2), SqlSelectOperation<TEntity, TResult1, TResult2>>
		where TEntity : class
	{
		private readonly IResultMapper<TResult1> _result1Mapper;
		private readonly IResultMapper<TResult2> _result2Mapper;

		public SqlSelectOperation(SqlDataSet<TEntity> dataSet, IQueryProvider queryProvider,
			SqlSelectBuilder<TEntity> queryBuilder,
			IResultMapper<TResult1> result1Mapper,
			IResultMapper<TResult2> result2Mapper) :
			base(dataSet, queryProvider, queryBuilder)
		{
			_result1Mapper = result1Mapper;
			_result2Mapper = result2Mapper;
		}

		public SqlSelectOperation<TEntity, TResult1, TResult2, TEntity> Select()
		{
			return new SqlSelectOperation<TEntity, TResult1, TResult2, TEntity>(DataSet, QueryProvider,
				QueryBuilder,
				_result1Mapper, _result2Mapper,
				QueryBuilder.Select(DataSet.Bindings.GetProjectionModel<TEntity>()));
		}

		public SqlSelectOperation<TEntity, TResult1, TResult2, TView> Select<TView>(
			DataModelBinding<TypeModel<TEntity>, PropertyField, TypeModel<TView>, PropertyField> binding = null
			)
			where TView : class
		{
			return new SqlSelectOperation<TEntity, TResult1, TResult2, TView>(DataSet, QueryProvider,
				QueryBuilder,
				_result1Mapper, _result2Mapper,
				QueryBuilder.Select(DataSet.Bindings.GetProjectionModel(binding)));
		}

		public SqlSelectOperation<TEntity, TResult1, TResult2, TExpr> Select<TExpr>(Expression<Func<TEntity, TExpr>> expression)
		{
			return new SqlSelectOperation<TEntity, TResult1, TResult2, TExpr>(DataSet, QueryProvider,
				QueryBuilder,
				_result1Mapper, _result2Mapper,
				QueryBuilder.Select(expression));
		}

		protected override void InjectSingle(QueryResult queryResult, (TResult1, TResult2) instance)
		{
			_result1Mapper.InjectSingle(queryResult, instance.Item1);
			_result2Mapper.InjectSingle(queryResult, instance.Item2);
		}

		protected override (TResult1, TResult2) MapSingle(QueryResult queryResult)
			=> (_result1Mapper.MapSingle(queryResult), _result2Mapper.MapSingle(queryResult));
	}

	public class SqlSelectOperation<TEntity, TResult1, TResult2, TResult3> : SqlSelectOperationBase<TEntity, (TResult1, TResult2, TResult3), SqlSelectOperation<TEntity, TResult1, TResult2, TResult3>>
		where TEntity : class
	{
		private readonly IResultMapper<TResult1> _result1Mapper;
		private readonly IResultMapper<TResult2> _result2Mapper;
		private readonly IResultMapper<TResult3> _result3Mapper;

		public SqlSelectOperation(SqlDataSet<TEntity> dataSet, IQueryProvider queryProvider,
			SqlSelectBuilder<TEntity> queryBuilder,
			IResultMapper<TResult1> result1Mapper,
			IResultMapper<TResult2> result2Mapper,
			IResultMapper<TResult3> result3Mapper) :
			base(dataSet, queryProvider, queryBuilder)
		{
			_result1Mapper = result1Mapper;
			_result2Mapper = result2Mapper;
			_result3Mapper = result3Mapper;
		}

		protected override void InjectSingle(QueryResult queryResult, (TResult1, TResult2, TResult3) instance)
		{
			_result1Mapper.InjectSingle(queryResult, instance.Item1);
			_result2Mapper.InjectSingle(queryResult, instance.Item2);
			_result3Mapper.InjectSingle(queryResult, instance.Item3);
		}

		protected override (TResult1, TResult2, TResult3) MapSingle(QueryResult queryResult)
			=> (_result1Mapper.MapSingle(queryResult), _result2Mapper.MapSingle(queryResult),
				_result3Mapper.MapSingle(queryResult));
	}
}
