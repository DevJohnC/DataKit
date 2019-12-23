using DataKit.SQL.Providers;
using MySql.Data.MySqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace DataKit.SQL.MySQL
{
	public class MySQLDataProvider : DataProviderBase
	{
		public const string PROVIDER_NAME = "MySQL/MariaDB";

		public static MySQLDataProvider Create(string host, string user, string pass, string database)
		{
			return new MySQLDataProvider(new MySqlConnectionStringBuilder
			{
				Server = host,
				UserID = user,
				Password = pass,
				Database = database
			});
		}

		public static MySQLDataProvider CreateWithoutSSL(string host, string user, string pass, string database)
		{
			return new MySQLDataProvider(new MySqlConnectionStringBuilder
			{
				Server = host,
				UserID = user,
				Password = pass,
				Database = database,
				SslMode = MySqlSslMode.None
			});
		}

		public override string ProviderName => PROVIDER_NAME;

		private readonly string _connectionString;

		public MySQLDataProvider(string connectionString)
		{
			_connectionString = connectionString;
		}

		public MySQLDataProvider(MySqlConnectionStringBuilder connectionStringBuilder) :
			this(connectionStringBuilder.ConnectionString)
		{
		}

		protected override QueryWriter CreateQueryWriter()
		{
			return new MySQLQueryWriter();
		}

		protected override IConnectionLease OpenConnection()
		{
			var connection = new MySqlConnection(_connectionString);
			connection.Open();
			return new ClosingConnectionLease(connection);
		}

		protected override async Task<IConnectionLease> OpenConnectionAsync(CancellationToken cancellationToken)
		{
			var connection = new MySqlConnection(_connectionString);
			await connection.OpenAsync(cancellationToken);
			return new ClosingConnectionLease(connection);
		}
	}
}
