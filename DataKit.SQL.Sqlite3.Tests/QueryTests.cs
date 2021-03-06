﻿using DataKit.SQL.Providers;
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

		[TestMethod]
		public void Can_Commit_In_Transaction()
		{
			using (var dataProvider = Sqlite3DataProvider.InMemory())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw(
					"CREATE TABLE TestTable (Value TEXT)"
					));

				using (var transaction = dataProvider.CreateTransaction())
				{
					transaction.ExecuteNonQuery(Sqlite3QueryExpression.Raw(
						"INSERT INTO TestTable VALUES ('Hello World')"
					));
					transaction.Commit();
				}

				using (var queryResult = dataProvider.ExecuteReader(QueryExpression.Select(
					projection: new[] { QueryExpression.All() },
					from: QueryExpression.Table("TestTable")
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(queryResult.Read());
					Assert.AreEqual("Hello World", queryResult.GetString(0));
				}
			}
		}

		[TestMethod]
		public void Can_Rollback_In_Transaction()
		{
			using (var dataProvider = Sqlite3DataProvider.InMemory())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw(
					"CREATE TABLE TestTable (Value TEXT)"
					));

				using (var transaction = dataProvider.CreateTransaction())
				{
					transaction.ExecuteNonQuery(Sqlite3QueryExpression.Raw(
						"INSERT INTO TestTable VALUES ('Hello World')"
					));
					transaction.Rollback();
				}

				using (var queryResult = dataProvider.ExecuteReader(QueryExpression.Select(
					projection: new[] { QueryExpression.All() },
					from: QueryExpression.Table("TestTable")
					)))
				{
					Assert.IsFalse(queryResult.HasRows);
				}
			}
		}

		[TestMethod]
		public void Can_Rollback_By_Disposing_Transaction()
		{
			using (var dataProvider = Sqlite3DataProvider.InMemory())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw(
					"CREATE TABLE TestTable (Value TEXT)"
					));

				using (var transaction = dataProvider.CreateTransaction())
				{
					transaction.ExecuteNonQuery(Sqlite3QueryExpression.Raw(
						"INSERT INTO TestTable VALUES ('Hello World')"
					));
				}

				using (var queryResult = dataProvider.ExecuteReader(QueryExpression.Select(
					projection: new[] { QueryExpression.All() },
					from: QueryExpression.Table("TestTable")
					)))
				{
					Assert.IsFalse(queryResult.HasRows);
				}
			}
		}

		[TestMethod]
		public void Can_Select_Concat_Values()
		{
			using (var dataProvider = Sqlite3DataProvider.InMemory())
			{
				using (var queryResult = dataProvider.ExecuteReader(QueryExpression.Select(
					projection: new[] { QueryExpression.Concat(
						QueryExpression.Value("Hello"),
						QueryExpression.Value(" "),
						QueryExpression.Value("World")
						) }
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(queryResult.Read());
					Assert.AreEqual("Hello World", queryResult.GetString(0));
				}
			}
		}

		[TestMethod]
		public void Can_Query_For_Last_Id()
		{
			using (var dataProvider = Sqlite3DataProvider.InMemory())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw(
					"CREATE TABLE TestTable (Id INT, Value TEXT, CONSTRAINT [PK] PRIMARY KEY (Id))"
					));

				var table = QueryExpression.Table("TestTable");
				using (var queryResult = dataProvider.ExecuteReader(QueryExpression.Many(
					QueryExpression.Insert(
						table,
						new[] { QueryExpression.Column("Value") },
						new[] { QueryExpression.Value("Hello World") },
						new[] { QueryExpression.Value("Hello World") }
						),
					QueryExpression.Select(
						projection: new[] { QueryExpression.LastInsertedId() },
						from: table
						)
					)))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(queryResult.Read());
					Assert.AreEqual(2, queryResult.GetInt32(0));
				}
			}
		}
	}
}
