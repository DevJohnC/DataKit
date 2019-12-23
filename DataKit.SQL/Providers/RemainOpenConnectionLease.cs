using System;
using System.Data.Common;

namespace DataKit.SQL.Providers
{
	public struct RemainOpenConnectionLease : IConnectionLease
	{
		public RemainOpenConnectionLease(DbConnection connection)
		{
			Connection = connection ?? throw new ArgumentNullException(nameof(connection));
		}

		public DbConnection Connection { get; }

		public void Dispose()
		{
		}
	}
}
