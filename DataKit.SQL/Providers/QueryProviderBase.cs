using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using DataKit.SQL.QueryExpressions;

namespace DataKit.SQL.Providers
{
	public abstract class QueryProviderBase : IQueryProvider
	{
		protected abstract QueryWriter CreateQueryWriter();

		protected virtual (string Sql, ParameterBag parameters) ConvertQuery(QueryExpression queryExpression)
		{
			var converter = new QueryConverter();
			var writer = CreateQueryWriter();
			converter.WriteQuery(writer, queryExpression);
			return (writer.ToString(), writer.Parameters);
		}

		protected abstract DbConnection OpenConnection();
		protected abstract Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken);

		protected virtual DbCommand CreateCommand(DbConnection dbConnection, string sql, ParameterBag parameters)
		{
			var command = dbConnection.CreateCommand();
			command.CommandText = sql;
			if (parameters != null)
			{
				foreach (var kvp in parameters)
				{
					var parameter = command.CreateParameter();
					parameter.ParameterName = kvp.Key;
					parameter.Value = kvp.Value ?? DBNull.Value;
					command.Parameters.Add(parameter);
				}
			}
			return command;
		}

		public int ExecuteNonQuery(ExecutableQueryExpression query)
		{
			var (sql, parameters) = ConvertQuery(query);
			using (var connection = OpenConnection())
			using (var command = CreateCommand(connection, sql, parameters))
			{
				return command.ExecuteNonQuery();
			}
		}

		public async Task<int> ExecuteNonQueryAsync(ExecutableQueryExpression query, CancellationToken cancellationToken = default)
		{
			var (sql, parameters) = ConvertQuery(query);
			using (var connection = await OpenConnectionAsync(cancellationToken))
			using (var command = CreateCommand(connection, sql, parameters))
			{
				return await command.ExecuteNonQueryAsync(cancellationToken);
			}
		}

		public virtual QueryResult ExecuteReader(ExecutableQueryExpression query)
		{
			var (sql, parameters) = ConvertQuery(query);
			var connection = OpenConnection();
			var command = CreateCommand(connection, sql, parameters);
			try
			{
				var reader = command.ExecuteReader();
				return new QueryResult(command, reader, connection);
			}
			catch
			{
				command.Dispose();
				connection.Dispose();
				throw;
			}
		}

		public virtual async Task<QueryResult> ExecuteReaderAsync(ExecutableQueryExpression query, CancellationToken cancellationToken = default)
		{
			var (sql, parameters) = ConvertQuery(query);
			var connection = await OpenConnectionAsync(cancellationToken);
			var command = CreateCommand(connection, sql, parameters);
			try
			{
				var reader = await command.ExecuteReaderAsync(cancellationToken);
				return new QueryResult(command, reader, connection);
			}
			catch
			{
				command.Dispose();
				connection.Dispose();
				throw;
			}
		}
	}
}
