using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace DataKit.SQL.Providers
{
	public abstract class DataProviderBase : QueryProviderBase, IDataProvider
	{
		public ITransaction CreateTransaction()
		{
			var connectionLease = OpenConnection();
			var transaction = connectionLease.Connection.BeginTransaction();
			return new Transaction(connectionLease, transaction, this);
		}

		public ITransaction CreateTransaction(IsolationLevel isolationLevel)
		{
			var connectionLease = OpenConnection();
			var transaction = connectionLease.Connection.BeginTransaction(isolationLevel);
			return new Transaction(connectionLease, transaction, this);
		}

		public async Task<ITransaction> CreateTransactionAsync(CancellationToken cancellationToken = default)
		{
			var connectionLease = await OpenConnectionAsync(cancellationToken);
			var transaction = connectionLease.Connection.BeginTransaction();
			return new Transaction(connectionLease, transaction, this);
		}

		public async Task<ITransaction> CreateTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
		{
			var connectionLease = await OpenConnectionAsync(cancellationToken);
			var transaction = connectionLease.Connection.BeginTransaction(isolationLevel);
			return new Transaction(connectionLease, transaction, this);
		}
	}
}
