using DataKit.SQL.Providers;
using Npgsql;
using System.Threading;
using System.Threading.Tasks;

namespace DataKit.SQL.Postgresql
{
	public class PostgresqlDataProvider : DataProviderBase
	{
		public const string PROVIDER_NAME = "Postgresql";

		public static PostgresqlDataProvider Create(string host, string user, string pass, string database)
		{
			return new PostgresqlDataProvider(new NpgsqlConnectionStringBuilder {
				Host = host,
				Username = user,
				Password = pass,
				Database = database
			});
		}

		public override string ProviderName => PROVIDER_NAME;

		private readonly string _connectionString;

		public PostgresqlDataProvider(string connectionString)
		{
			_connectionString = connectionString;
		}

		public PostgresqlDataProvider(NpgsqlConnectionStringBuilder connectionStringBuilder) :
			this(connectionStringBuilder.ConnectionString)
		{
		}

		protected override QueryWriter CreateQueryWriter()
		{
			return new PostgresqlQueryWriter();
		}

		protected override IConnectionLease OpenConnection()
		{
			var connection = new NpgsqlConnection(_connectionString);
			connection.Open();
			return new ClosingConnectionLease(connection);
		}

		protected override async Task<IConnectionLease> OpenConnectionAsync(CancellationToken cancellationToken)
		{
			var connection = new NpgsqlConnection(_connectionString);
			await connection.OpenAsync(cancellationToken);
			return new ClosingConnectionLease(connection);
		}
	}
}
