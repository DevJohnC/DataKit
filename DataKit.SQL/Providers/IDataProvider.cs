using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace DataKit.SQL.Providers
{
	public interface IDataProvider : IQueryProvider
	{
		ITransaction CreateTransaction();
		ITransaction CreateTransaction(IsolationLevel isolationLevel);
		Task<ITransaction> CreateTransactionAsync(CancellationToken cancellationToken = default);
		Task<ITransaction> CreateTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default);
	}
}
