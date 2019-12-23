using DataKit.SQL.Providers;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace DataKit.SQL.SqlServer
{
	public class SqlServerDataProvider : DataProviderBase
	{
		public const string PROVIDER_NAME = "Microsoft SQL Server";

		public static SqlServerDataProvider Create(string host, string user, string pass, string database)
		{
			return new SqlServerDataProvider(new SqlConnectionStringBuilder
			{
				DataSource = host,
				UserID = user,
				Password = pass,
				InitialCatalog = database
			});
		}

		public override string ProviderName => PROVIDER_NAME;

		private readonly string _connectionString;

		public SqlServerDataProvider(string connectionString)
		{
			_connectionString = connectionString;
		}

		public SqlServerDataProvider(SqlConnectionStringBuilder connectionStringBuilder) :
			this(connectionStringBuilder.ConnectionString)
		{
		}

		protected override QueryWriter CreateQueryWriter()
		{
			return new SqlServerQueryWriter();
		}

		protected override IConnectionLease OpenConnection()
		{
			var connection = new SqlConnection(_connectionString);
			connection.Open();
			return new ClosingConnectionLease(connection);
		}

		protected override async Task<IConnectionLease> OpenConnectionAsync(CancellationToken cancellationToken)
		{
			var connection = new SqlConnection(_connectionString);
			await connection.OpenAsync(cancellationToken);
			return new ClosingConnectionLease(connection);
		}
	}
}
