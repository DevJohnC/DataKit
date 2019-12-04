using Silk.Data.SQL.Providers;
using System;

namespace DataKit.ORM
{
	public class Transaction : IDisposable
	{
		private readonly ITransaction _dbTransaction;

		public Transaction(ITransaction dbTransaction)
		{
			_dbTransaction = dbTransaction;
		}

		public void Commit()
		{
			_dbTransaction.Commit();
		}

		public void Rollback()
		{
			_dbTransaction.Rollback();
		}

		public void Dispose()
		{
			_dbTransaction.Dispose();
		}
	}
}
