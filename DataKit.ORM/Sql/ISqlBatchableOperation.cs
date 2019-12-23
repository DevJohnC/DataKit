﻿using DataKit.SQL.Providers;
using DataKit.SQL.QueryExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataKit.ORM.Sql
{
	public interface ISqlBatchableOperation
	{
		ExecutableQueryExpression BuildQuery();
	}

	public interface ISqlBatchableOperation<TResult>
	{
		ExecutableQueryExpression BuildQuery();
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
