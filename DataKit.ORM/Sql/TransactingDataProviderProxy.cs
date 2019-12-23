using DataKit.SQL.Providers;
using DataKit.SQL.QueryExpressions;
using System;
using System.Data;
using System.Threading;
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
			if (_activeQueryProvider is IDisposable disposableActive)
				disposableActive.Dispose();
			if (_provider is IDisposable disposableProvider)
				disposableProvider.Dispose();
		}

		public ITransaction CreateTransaction()
		{
			var transaction = new Transaction(_provider.CreateTransaction(), this);
			_activeQueryProvider = transaction;
			return transaction;
		}

		public ITransaction CreateTransaction(IsolationLevel isolationLevel)
		{
			var transaction = new Transaction(_provider.CreateTransaction(isolationLevel), this);
			_activeQueryProvider = transaction;
			return transaction;
		}

		public async Task<ITransaction> CreateTransactionAsync()
		{
			var transaction = new Transaction(await _provider.CreateTransactionAsync(), this);
			_activeQueryProvider = transaction;
			return transaction;
		}

		public async Task<ITransaction> CreateTransactionAsync(IsolationLevel isolationLevel)
		{
			var transaction = new Transaction(await _provider.CreateTransactionAsync(isolationLevel), this);
			_activeQueryProvider = transaction;
			return transaction;
		}

		private void SwitchOffTransaction()
		{
			_activeQueryProvider = _provider;
		}

		public int ExecuteNonQuery(ExecutableQueryExpression query, ParameterBag parameters = null)
			=> _activeQueryProvider.ExecuteNonQuery(query, parameters);

		public Task<int> ExecuteNonQueryAsync(ExecutableQueryExpression query, ParameterBag parameters = null, CancellationToken cancellationToken = default)
			=> _activeQueryProvider.ExecuteNonQueryAsync(query, parameters);

		public QueryResult ExecuteReader(ExecutableQueryExpression query, ParameterBag parameters = null)
			=> _activeQueryProvider.ExecuteReader(query, parameters);

		public Task<QueryResult> ExecuteReaderAsync(ExecutableQueryExpression query, ParameterBag parameters = null, CancellationToken cancellationToken = default)
			=> _activeQueryProvider.ExecuteReaderAsync(query, parameters);

		private class Transaction : ITransaction
		{
			private readonly ITransaction _transaction;
			private readonly TransactingDataProviderProxy _dataProviderProxy;

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

			public int ExecuteNonQuery(ExecutableQueryExpression query, ParameterBag parameters = null)
				=> _transaction.ExecuteNonQuery(query, parameters);

			public Task<int> ExecuteNonQueryAsync(ExecutableQueryExpression query, ParameterBag parameters = null, CancellationToken cancellationToken = default)
				=> _transaction.ExecuteNonQueryAsync(query, parameters);

			public QueryResult ExecuteReader(ExecutableQueryExpression query, ParameterBag parameters = null)
				=> _transaction.ExecuteReader(query, parameters);

			public Task<QueryResult> ExecuteReaderAsync(ExecutableQueryExpression query, ParameterBag parameters = null, CancellationToken cancellationToken = default)
				=> _transaction.ExecuteReaderAsync(query, parameters);

			public void Rollback()
				=> _transaction.Rollback();
		}
	}
}
