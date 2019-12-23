using DataKit.SQL.QueryExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DataKit.SQL.Providers
{
	public interface IQueryProvider
	{
		int ExecuteNonQuery(ExecutableQueryExpression query, ParameterBag parameters = null);
		Task<int> ExecuteNonQueryAsync(ExecutableQueryExpression query, ParameterBag parameters = null, CancellationToken cancellationToken = default);

		QueryResult ExecuteReader(ExecutableQueryExpression query, ParameterBag parameters = null);
		Task<QueryResult> ExecuteReaderAsync(ExecutableQueryExpression query, ParameterBag parameters = null, CancellationToken cancellationToken = default);
	}
}
