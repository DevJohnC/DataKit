using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataKit.ORM.Sql
{
	public interface ISqlBatchableOperation
	{
		QueryExpression BuildQuery();
	}

	public interface ISqlBatchableOperation<TResult>
	{
		QueryExpression BuildQuery();
		ISqlBatchProcessor GetInjectBatchProcessor(TResult instance);
		ISqlBatchProcessor<TResult> GetSingleBatchProcessor();
		ISqlBatchProcessor<IReadOnlyList<TResult>> GetListBatchProcessor();
	}

	public interface ISqlBatchProcessor
	{
		void ProcessResult(QueryResult queryResult);
		Task ProcessResultAsync(QueryResult queryResult);
	}

	public interface ISqlBatchProcessor<TResult> : ISqlBatchProcessor
	{
		DeferredResult<TResult> Result { get; }
	}
}
