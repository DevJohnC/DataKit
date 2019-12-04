using System.Threading.Tasks;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.Providers;
using Silk.Data.SQL.Queries;
using Silk.Data.SQL.SQLite3;

namespace DataKit.ORM.UnitTests
{
	public static class DataProvider
	{
		public static IDataProvider CreateTestProvider()
		{
			return new DebuggingDataProvider();
		}
	}

	public class DebuggingDataProvider : IDataProvider
	{
		private readonly SQLite3DataProvider _provider;

		public string ProviderName => "DebugProvider";

		public DebuggingDataProvider()
		{
			_provider = new SQLite3DataProvider("Data Source=:memory:;Mode=Memory");
		}

		public ITransaction CreateTransaction()
			=> _provider.CreateTransaction();

		public Task<ITransaction> CreateTransactionAsync()
			=> _provider.CreateTransactionAsync();

		public int ExecuteNonQuery(QueryExpression queryExpression)
		{
			var debugQuery = _provider.ConvertExpressionToQuery(queryExpression);
			return _provider.ExecuteNonQuery(queryExpression);
		}

		public Task<int> ExecuteNonQueryAsync(QueryExpression queryExpression)
		{
			var debugQuery = _provider.ConvertExpressionToQuery(queryExpression);
			return _provider.ExecuteNonQueryAsync(queryExpression);
		}

		public QueryResult ExecuteReader(QueryExpression queryExpression)
		{
			var debugQuery = _provider.ConvertExpressionToQuery(queryExpression);
			return _provider.ExecuteReader(queryExpression);
		}

		public Task<QueryResult> ExecuteReaderAsync(QueryExpression queryExpression)
		{
			var debugQuery = _provider.ConvertExpressionToQuery(queryExpression);
			return _provider.ExecuteReaderAsync(queryExpression);
		}

		public void Dispose()
			=> _provider.Dispose();
	}
}
