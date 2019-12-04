using DataKit.ORM.Sql;
using DataKit.ORM.Sql.Expressions;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.Providers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IQueryProvider = Silk.Data.SQL.Providers.IQueryProvider;

namespace DataKit.ORM
{
	public class Batch
	{
		private readonly List<BatchCollection> _batches =
			new List<BatchCollection>();
		private BatchCollection _topBatch;

		private SqlBatchCollection GetSqlBatchCollection(IQueryProvider queryProvider)
		{
			if (_topBatch is SqlBatchCollection && ReferenceEquals(_topBatch?.ExecutorReference, queryProvider))
				return _topBatch as SqlBatchCollection;

			_topBatch = new SqlBatchCollection(queryProvider);
			_batches.Add(_topBatch);
			return _topBatch as SqlBatchCollection;
		}

		public Batch Insert<TEntity>(SqlInsertOperation<TEntity> insertOperation)
			where TEntity : class
		{
			GetSqlBatchCollection(insertOperation.QueryProvider)
				.Add(insertOperation);
			return this;
		}

		public Batch Update<TEntity>(SqlUpdateOperation<TEntity> updateOperation)
			where TEntity : class
		{
			GetSqlBatchCollection(updateOperation.QueryProvider)
				.Add(updateOperation);
			return this;
		}

		public Batch Delete<TEntity>(SqlDeleteOperation<TEntity> deleteOperation)
			where TEntity : class
		{
			GetSqlBatchCollection(deleteOperation.QueryProvider)
				.Add(deleteOperation);
			return this;
		}

		public Batch List<TEntity, TResult>(SqlSelectOperation<TEntity, TResult> selectOperation, out DeferredResult<IReadOnlyList<TResult>> results)
			where TEntity : class
		{
			GetSqlBatchCollection(selectOperation.QueryProvider)
				.Add(selectOperation, out results);
			return this;
		}

		public Batch Single<TEntity, TResult>(SqlSelectOperation<TEntity, TResult> selectOperation, out DeferredResult<TResult> result)
			where TEntity : class
		{
			GetSqlBatchCollection(selectOperation.QueryProvider)
				.Add(selectOperation, out result);
			return this;
		}

		public Batch Inject<TEntity, TResult>(SqlSelectOperation<TEntity, TResult> selectOperation, TResult instance)
			where TEntity : class
			where TResult : class
		{
			GetSqlBatchCollection(selectOperation.QueryProvider)
				.Add(selectOperation, instance);
			return this;
		}

		public void Execute()
		{
			foreach (var batch in _batches)
				batch.Execute();
		}

		public async Task ExecuteAsync()
		{
			foreach (var batch in _batches)
				await batch.ExecuteAsync();
		}

		public static Batch Create() => new Batch();

		private abstract class BatchCollection
		{
			public abstract object ExecutorReference { get; }

			public abstract void Execute();

			public abstract Task ExecuteAsync();
		}

		private class SqlBatchCollection : BatchCollection
		{
			private readonly IQueryProvider _queryProvider;

			private readonly List<Query> _queries =
				new List<Query>();

			public override object ExecutorReference => _queryProvider;

			public SqlBatchCollection(IQueryProvider queryProvider)
			{
				_queryProvider = queryProvider;
			}

			public void Add(ISqlBatchableOperation batchableOperation)
			{
				_queries.Add(new Query(batchableOperation.BuildQuery(), null));
			}

			public void Add<TResult>(ISqlBatchableOperation<TResult> batchableOperation, TResult instance)
				where TResult : class
			{
				var resultProcessor = batchableOperation.GetInjectBatchProcessor(instance);
				_queries.Add(new Query(batchableOperation.BuildQuery(), resultProcessor));
			}

			public void Add<TResult>(ISqlBatchableOperation<TResult> batchableOperation, out DeferredResult<TResult> result)
			{
				var resultProcessor = batchableOperation.GetSingleBatchProcessor();
				result = resultProcessor.Result;
				_queries.Add(new Query(batchableOperation.BuildQuery(), resultProcessor));
			}

			public void Add<TResult>(ISqlBatchableOperation<TResult> batchableOperation, out DeferredResult<IReadOnlyList<TResult>> result)
			{
				var resultProcessor = batchableOperation.GetListBatchProcessor();
				result = resultProcessor.Result;
				_queries.Add(new Query(batchableOperation.BuildQuery(), resultProcessor));
			}

			private QueryExpression BuildQuery()
				=> new CompositeQueryExpression(_queries.Select(q => q.QueryExpression));

			public override void Execute()
			{
				using (var queryResult = _queryProvider.ExecuteReader(BuildQuery()))
				{
					foreach (var processor in _queries.Select(q => q.Processor).Where(q => q != null))
					{
						processor.ProcessResult(queryResult);
						queryResult.NextResult();
					}
				}
			}

			public override async Task ExecuteAsync()
			{
				using (var queryResult = await _queryProvider.ExecuteReaderAsync(BuildQuery()))
				{
					foreach (var processor in _queries.Select(q => q.Processor).Where(q => q != null))
					{
						await processor.ProcessResultAsync(queryResult);
						await queryResult.NextResultAsync();
					}
				}
			}

			private class Query
			{
				public Query(QueryExpression queryExpression, ISqlBatchProcessor processor)
				{
					QueryExpression = queryExpression;
					Processor = processor;
				}

				public QueryExpression QueryExpression { get; }
				public ISqlBatchProcessor Processor { get; }
			}
		}
	}
}
