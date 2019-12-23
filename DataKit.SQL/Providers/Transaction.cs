using DataKit.SQL.QueryExpressions;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace DataKit.SQL.Providers
{
	public class Transaction : ITransaction
	{
		private readonly IConnectionLease _connectionLease;
		private readonly DbTransaction _transaction;
		private readonly IQueryProviderCommon _queryProvider;

		public Transaction(
			IConnectionLease connectionLease,
			DbTransaction transaction,
			IQueryProviderCommon queryProvider
			)
		{
			_connectionLease = connectionLease;
			_transaction = transaction;
			_queryProvider = queryProvider;
		}

		public void Commit()
			=> _transaction.Commit();

		public void Rollback()
			=> _transaction.Rollback();

		public void Dispose()
		{
			_transaction.Dispose();
			_connectionLease.Dispose();
		}

		public int ExecuteNonQuery(ExecutableQueryExpression query, ParameterBag parameters = null)
		{
			var (sql, autoParameters) = _queryProvider.ConvertQuery(query);
			using (var command = _queryProvider.CreateCommand(_connectionLease.Connection, sql, ParameterBag.Combine(autoParameters, parameters)))
			{
				command.Transaction = _transaction;
				return command.ExecuteNonQuery();
			}
		}

		public Task<int> ExecuteNonQueryAsync(ExecutableQueryExpression query, ParameterBag parameters = null, CancellationToken cancellationToken = default)
		{
			var (sql, autoParameters) = _queryProvider.ConvertQuery(query);
			using (var command = _queryProvider.CreateCommand(_connectionLease.Connection, sql, ParameterBag.Combine(autoParameters, parameters)))
			{
				command.Transaction = _transaction;
				return command.ExecuteNonQueryAsync();
			}
		}

		public QueryResult ExecuteReader(ExecutableQueryExpression query, ParameterBag parameters = null)
		{
			var (sql, autoParameters) = _queryProvider.ConvertQuery(query);
			var command = _queryProvider.CreateCommand(_connectionLease.Connection, sql, ParameterBag.Combine(autoParameters, parameters));
			try
			{
				command.Transaction = _transaction;
				var reader = command.ExecuteReader();
				return new QueryResult(command, reader, new RemainOpenConnectionLease(_connectionLease.Connection));
			}
			catch
			{
				command.Dispose();
				throw;
			}
		}

		public async Task<QueryResult> ExecuteReaderAsync(ExecutableQueryExpression query, ParameterBag parameters = null, CancellationToken cancellationToken = default)
		{
			var (sql, autoParameters) = _queryProvider.ConvertQuery(query);
			var command = _queryProvider.CreateCommand(_connectionLease.Connection, sql, ParameterBag.Combine(autoParameters, parameters));
			try
			{
				command.Transaction = _transaction;
				var reader = await command.ExecuteReaderAsync();
				return new QueryResult(command, reader, new RemainOpenConnectionLease(_connectionLease.Connection));
			}
			catch
			{
				command.Dispose();
				throw;
			}
		}
	}
}
