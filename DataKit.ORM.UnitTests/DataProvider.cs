using DataKit.SQL.Sqlite3;

namespace DataKit.ORM.UnitTests
{
	public static class DataProvider
	{
		public static Sqlite3DataProvider CreateTestProvider()
		{
			return Sqlite3DataProvider.InMemory();
		}
	}
}
