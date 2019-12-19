using DataKit.SQL.Providers;
using DataKit.SQL.QueryExpressions;
using Microsoft.Data.Sqlite;
using System;
using System.Data.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DataKit.SQL.Sqlite3
{
	public class Sqlite3DataProvider : DataProviderBase, IDisposable
	{
		public static Sqlite3DataProvider InMemory(bool convertGuidsToText = false)
		{
			return new Sqlite3DataProvider(
				new SqliteConnectionStringBuilder { Mode = SqliteOpenMode.Memory, DataSource = ":memory:" },
				convertGuidsToText);
		}

		public static Sqlite3DataProvider FromFile(string filePath, bool convertGuidsToText = false)
		{
			return new Sqlite3DataProvider(
				new SqliteConnectionStringBuilder { Mode = SqliteOpenMode.ReadWriteCreate, DataSource = filePath },
				convertGuidsToText);
		}

		public static Sqlite3DataProvider FromFile(FileInfo file, bool convertGuidsToText = false)
			=> FromFile(file.FullName, convertGuidsToText);

		private readonly bool _convertGuidsToText;
		private readonly string _connectionString;
		private readonly DbConnection _connectionOverride;

		public Sqlite3DataProvider(
			SqliteConnectionStringBuilder connectionStringBuilder, bool convertGuidsToText)
		{
			_convertGuidsToText = convertGuidsToText;
			_connectionString = connectionStringBuilder.ConnectionString;
			if (connectionStringBuilder.Mode == SqliteOpenMode.Memory)
			{
				_connectionOverride = CreateConnection();
			}
		}

		private DbConnection CreateConnection()
		{
			var connection = new SqliteConnection(_connectionString);
			connection.Open();
			return connection;
		}

		protected override DbConnection OpenConnection()
			=> _connectionOverride ?? CreateConnection();

		protected override Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken)
			=> Task.FromResult(OpenConnection());

		protected override QueryWriter CreateQueryWriter()
		{
			return new Sqlite3QueryWriter(_convertGuidsToText);
		}

		public override QueryResult ExecuteReader(ExecutableQueryExpression query)
		{
			var (sql, parameters) = ConvertQuery(query);
			var connection = OpenConnection();
			var command = CreateCommand(connection, sql, parameters);
			try
			{
				var reader = command.ExecuteReader();
				return new QueryResult(command, reader, _connectionOverride == null ? connection : null);
			}
			catch
			{
				command.Dispose();
				connection.Dispose();
				throw;
			}
		}

		public override async Task<QueryResult> ExecuteReaderAsync(ExecutableQueryExpression query, CancellationToken cancellationToken = default)
		{
			var (sql, parameters) = ConvertQuery(query);
			var connection = await OpenConnectionAsync(cancellationToken);
			var command = CreateCommand(connection, sql, parameters);
			try
			{
				var reader = await command.ExecuteReaderAsync(cancellationToken);
				return new QueryResult(command, reader, _connectionOverride == null ? connection : null);
			}
			catch
			{
				command.Dispose();
				connection.Dispose();
				throw;
			}
		}

		public void Dispose()
		{
			_connectionOverride?.Dispose();
		}
	}
}
