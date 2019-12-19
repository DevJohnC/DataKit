using System;
using System.Data.Common;

namespace DataKit.SQL.Providers
{
	public struct ClosingConnectionLease : IConnectionLease
	{
		public ClosingConnectionLease(DbConnection connection)
		{
			Connection = connection ?? throw new ArgumentNullException(nameof(connection));
		}

		public DbConnection Connection { get; }

		public void Dispose()
		{
			Connection.Dispose();
		}
	}
}
