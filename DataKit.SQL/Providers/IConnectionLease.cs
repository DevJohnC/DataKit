using System;
using System.Data.Common;

namespace DataKit.SQL.Providers
{
	public interface IConnectionLease : IDisposable
	{
		DbConnection Connection { get; }
	}
}
