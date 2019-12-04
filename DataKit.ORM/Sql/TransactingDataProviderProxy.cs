using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.Providers;
using Silk.Data.SQL.Queries;
using System.Threading.Tasks;

namespace DataKit.ORM.Sql
{
	public class TransactingDataProviderProxy : IQueryProvider
	{
		private readonly IDataProvider _provider;
		private IQueryProvider _activeQueryProvider;

		public string ProviderName => _provider.ProviderName;

		public TransactingDataProviderProxy(IDataProvider provider)
		{
			_provider = provider;
			_activeQueryProvider = _provider;
		}

		public void Dispose()
		{
			_activeQueryProvider.Dispose();
			_provider.Dispose();
		}

		public ITransaction CreateTransaction()
		{
			var transaction = new Transaction(_provider.CreateTransaction(), this);
			_activeQueryProvider = transaction;
			return transaction;
		}

		public async Task<ITransaction> CreateTransactionAsync()
		{
			var transaction = new Transaction(await _provider.CreateTransactionAsync(), this);
			_activeQueryProvider = transaction;
			return transaction;
		}

		private void SwitchOffTransaction()
		{
			_activeQueryProvider = _provider;
		}

		public int ExecuteNonQuery(QueryExpression queryExpression)
			=> _activeQueryProvider.ExecuteNonQuery(queryExpression);

		public Task<int> ExecuteNonQueryAsync(QueryExpression queryExpression)
			=> _activeQueryProvider.ExecuteNonQueryAsync(queryExpression);

		public QueryResult ExecuteReader(QueryExpression queryExpression)
			=> _activeQueryProvider.ExecuteReader(queryExpression);

		public Task<QueryResult> ExecuteReaderAsync(QueryExpression queryExpression)
			=> _activeQueryProvider.ExecuteReaderAsync(queryExpression);

		private class Transaction : ITransaction
		{
			private readonly ITransaction _transaction;
			private readonly TransactingDataProviderProxy _dataProviderProxy;

			public string ProviderName => _transaction.ProviderName;

			public Transaction(ITransaction transaction, TransactingDataProviderProxy dataProviderProxy)
			{
				_transaction = transaction;
				_dataProviderProxy = dataProviderProxy;
			}

			public void Commit()
				=> _transaction.Commit();

			public void Dispose()
			{
				_transaction.Dispose();
				_dataProviderProxy.SwitchOffTransaction();
			}

			public int ExecuteNonQuery(QueryExpression queryExpression)
				=> _transaction.ExecuteNonQuery(queryExpression);

			public Task<int> ExecuteNonQueryAsync(QueryExpression queryExpression)
				=> _transaction.ExecuteNonQueryAsync(queryExpression);

			public QueryResult ExecuteReader(QueryExpression queryExpression)
				=> _transaction.ExecuteReader(queryExpression);

			public Task<QueryResult> ExecuteReaderAsync(QueryExpression queryExpression)
				=> _transaction.ExecuteReaderAsync(queryExpression);

			public void Rollback()
				=> _transaction.Rollback();
		}
	}
}
