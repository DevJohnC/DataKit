using System;

namespace DataKit.SQL.Providers
{
	public interface ITransaction : IQueryProvider, IDisposable
	{
	}
}
