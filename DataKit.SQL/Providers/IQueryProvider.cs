using DataKit.SQL.QueryExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DataKit.SQL.Providers
{
	public interface IQueryProvider
	{
		int ExecuteNonQuery(ExecutableQueryExpression query);
		Task<int> ExecuteNonQueryAsync(ExecutableQueryExpression query, CancellationToken cancellationToken = default);

		QueryResult ExecuteReader(ExecutableQueryExpression query);
		Task<QueryResult> ExecuteReaderAsync(ExecutableQueryExpression query, CancellationToken cancellationToken = default);
	}
}
