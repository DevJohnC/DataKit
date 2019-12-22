using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using DataKit.SQL.QueryExpressions;

namespace DataKit.SQL.Providers
{
	public abstract class QueryProviderBase : IQueryProvider, IQueryProviderCommon
	{
		protected abstract QueryWriter CreateQueryWriter();

		protected virtual (string Sql, ParameterBag parameters) ConvertQuery(QueryExpression queryExpression)
		{
			var converter = new QueryConverter();
			var writer = CreateQueryWriter();
			converter.WriteQuery(writer, queryExpression);
			return (writer.ToString(), writer.Parameters);
		}

		protected abstract IConnectionLease OpenConnection();
		protected abstract Task<IConnectionLease> OpenConnectionAsync(CancellationToken cancellationToken);

		protected virtual DbCommand CreateCommand(DbConnection dbConnection, string sql, ParameterBag parameters)
		{
			var command = dbConnection.CreateCommand();
			command.CommandText = sql;
			if (parameters != null)
			{
				foreach (var kvp in parameters)
				{
					var parameter = command.CreateParameter();
					parameter.ParameterName = $"@{kvp.Key}";
					parameter.Value = kvp.Value ?? DBNull.Value;
					command.Parameters.Add(parameter);
				}
			}
			return command;
		}

		public int ExecuteNonQuery(ExecutableQueryExpression query, ParameterBag parameters = null)
		{
			var (sql, autoParameters) = ConvertQuery(query);
			using (var lease = OpenConnection())
			using (var command = CreateCommand(lease.Connection, sql, ParameterBag.Combine(autoParameters, parameters)))
			{
				return command.ExecuteNonQuery();
			}
		}

		public async Task<int> ExecuteNonQueryAsync(ExecutableQueryExpression query, ParameterBag parameters = null, CancellationToken cancellationToken = default)
		{
			var (sql, autoParameters) = ConvertQuery(query);
			using (var lease = await OpenConnectionAsync(cancellationToken))
			using (var command = CreateCommand(lease.Connection, sql, ParameterBag.Combine(autoParameters, parameters)))
			{
				return await command.ExecuteNonQueryAsync(cancellationToken);
			}
		}

		public virtual QueryResult ExecuteReader(ExecutableQueryExpression query, ParameterBag parameters = null)
		{
			var (sql, autoParameters) = ConvertQuery(query);
			var lease = OpenConnection();
			var command = CreateCommand(lease.Connection, sql, ParameterBag.Combine(autoParameters, parameters));
			try
			{
				var reader = command.ExecuteReader();
				return new QueryResult(command, reader, lease);
			}
			catch
			{
				command.Dispose();
				lease.Dispose();
				throw;
			}
		}

		public virtual async Task<QueryResult> ExecuteReaderAsync(ExecutableQueryExpression query, ParameterBag parameters = null, CancellationToken cancellationToken = default)
		{
			var (sql, autoParameters) = ConvertQuery(query);
			var lease = await OpenConnectionAsync(cancellationToken);
			var command = CreateCommand(lease.Connection, sql, ParameterBag.Combine(autoParameters, parameters));
			try
			{
				var reader = await command.ExecuteReaderAsync(cancellationToken);
				return new QueryResult(command, reader, lease);
			}
			catch
			{
				command.Dispose();
				lease.Dispose();
				throw;
			}
		}

		(string Sql, ParameterBag parameters) IQueryProviderCommon.ConvertQuery(QueryExpression queryExpression)
			=> ConvertQuery(queryExpression);

		DbCommand IQueryProviderCommon.CreateCommand(DbConnection dbConnection, string sql, ParameterBag parameters)
			=> CreateCommand(dbConnection, sql, parameters);
	}
}
