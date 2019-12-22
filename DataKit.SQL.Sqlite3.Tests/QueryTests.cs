using DataKit.SQL.Providers;
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
					"INSERT INTO TestTable VALUES ('Hello World')"
					));

				var table = QueryExpression.Table("TestTable");
				using (var queryResult = dataProvider.ExecuteReader(QueryExpression.Select(
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

		[TestMethod]
		public void Can_Query_With_Parameters()
		{
			using (var dataProvider = Sqlite3DataProvider.InMemory())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw(
					"CREATE TABLE TestTable (Value TEXT)"
					));
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw(
					"INSERT INTO TestTable VALUES ('Hello World')"
					));

				var parameters = new ParameterBag
				{
					{ "valueParameter", "Hello World" }
				};
				using (var queryResult = dataProvider.ExecuteReader(QueryExpression.Select(
					projection: new[] { QueryExpression.All() },
					from: QueryExpression.Table("TestTable"),
					where: QueryExpression.AreEqual(QueryExpression.Column("Value"), QueryExpression.Parameter("valueParameter"))
					), parameters))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(queryResult.Read());
					Assert.AreEqual("Hello World", queryResult.GetString(0));
				}
			}
		}

		[TestMethod]
		public void Can_Query_With_Values()
		{
			using (var dataProvider = Sqlite3DataProvider.InMemory())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw(
					"CREATE TABLE TestTable (Value TEXT)"
					));
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw(
					"INSERT INTO TestTable VALUES ('Hello World')"
					));

				using (var queryResult = dataProvider.ExecuteReader(QueryExpression.Select(
					projection: new[] { QueryExpression.All() },
					from: QueryExpression.Table("TestTable"),
					where: QueryExpression.AreEqual(QueryExpression.Column("Value"), QueryExpression.Value("Hello World"))
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
