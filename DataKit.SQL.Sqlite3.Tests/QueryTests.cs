using DataKit.SQL.QueryExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataKit.SQL.Sqlite3.Tests
{
	[TestClass]
	public class QueryTests
	{
		[TestMethod]
		public void Can_Query_For_Data()
		{
			using (var dataProvider = Sqlite3DataProvider.InMemory())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw(
					"CREATE TABLE TestTable (Value TEXT)"
					));
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw(
					"INSERT INTO TestTable Value='Hello World'"
					));

				var table = QueryExpression.Table("TestTable");
				using (var queryResult = dataProvider.ExecuteReader(QueryExpression.SelectStatement(
					projection: new[] { QueryExpression.Column("Value", table) },
					from: table
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(queryResult.Read());
					Assert.AreEqual("Hello World", queryResult.GetString(0));
				}
			}
		}
	}
}
