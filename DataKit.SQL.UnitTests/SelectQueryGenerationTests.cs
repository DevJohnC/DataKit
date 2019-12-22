using DataKit.SQL.QueryExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DataKit.SQL.UnitTests
{
	[TestClass]
	public class SelectQueryGenerationTests
	{
		[TestMethod]
		public void Can_Write_Multiple_Statements()
		{
			var queryExpression = 
				QueryExpression.Many(
				QueryExpression.Select(
					new[] { QueryExpression.Count() }
				),
				QueryExpression.Select(
					new[] { QueryExpression.Count() }
				));
			var sql = TestHelpers.ConvertToSql(queryExpression);
			Assert.AreEqual("SELECT COUNT(); SELECT COUNT();", sql);
		}

		[TestMethod]
		public void Can_Write_Select_With_Projection_Only()
		{
			var queryExpression = QueryExpression.Select(
				new[] { QueryExpression.Count() }
				);
			var sql = TestHelpers.ConvertToSql(queryExpression);
			Assert.AreEqual("SELECT COUNT();", sql);
		}

		[TestMethod]
		public void Can_Write_Select_With_Distinct_Count()
		{
			var queryExpression = QueryExpression.Select(
				new[] { QueryExpression.Count(
					QueryExpression.Distinct(QueryExpression.Column("Id"))
					) }
				);
			var sql = TestHelpers.ConvertToSql(queryExpression);
			Assert.AreEqual("SELECT COUNT(DISTINCT [Id]);", sql);
		}

		[TestMethod]
		public void Can_Write_Select_With_Multiple_Projections()
		{
			var queryExpression = QueryExpression.Select(
				new[] { QueryExpression.Column("Column1"), QueryExpression.Column("Column2") },
				from: QueryExpression.Table("TestTable")
				);
			var sql = TestHelpers.ConvertToSql(queryExpression);
			Assert.AreEqual("SELECT [Column1], [Column2] FROM [TestTable];", sql);
		}

		[TestMethod]
		public void Can_Write_Select_With_Alias()
		{
			var queryExpression = QueryExpression.Select(
				new[] { QueryExpression.As(QueryExpression.Column("TestColumn"), "aliasTest", out var _) },
				from: QueryExpression.Table("TestTable")
				);
			var sql = TestHelpers.ConvertToSql(queryExpression);
			Assert.AreEqual("SELECT [TestColumn] AS [aliasTest] FROM [TestTable];", sql);
		}

		[TestMethod]
		public void Can_Write_Select_With_In_Query()
		{
			var queryExpression = QueryExpression.Select(
				new[] { QueryExpression.IsIn(
					QueryExpression.Column("Column"),
					QueryExpression.Select(new[] { QueryExpression.Column("Id") }, from: QueryExpression.Table("OtherTable"))
					) },
				from: QueryExpression.Table("TestTable")
				);
			var sql = TestHelpers.ConvertToSql(queryExpression);
			Assert.AreEqual("SELECT [Column] IN (SELECT [Id] FROM [OtherTable]) FROM [TestTable];", sql);
		}

		[TestMethod]
		public void Can_Write_Select_With_Nested_Conditions()
		{
			var queryExpression = QueryExpression.Select(
				new[] { QueryExpression.All() },
				from: QueryExpression.Table("TestTable"),
				where: QueryExpression.AndAlso(
					QueryExpression.AreEqual(QueryExpression.Column("Left"), QueryExpression.Column("Right")),
					QueryExpression.OrElse(
						QueryExpression.AreEqual(QueryExpression.Column("Left"), QueryExpression.Column("Right")),
						QueryExpression.AreNotEqual(QueryExpression.Column("Left"), QueryExpression.Column("Right"))
					))
				);
			var sql = TestHelpers.ConvertToSql(queryExpression);
			Assert.AreEqual("SELECT * FROM [TestTable] WHERE ([Left] = [Right] AND ([Left] = [Right] OR [Left] != [Right]));", sql);
		}

		[TestMethod]
		public void Can_Write_Select_With_Parameter_Reference()
		{
			var queryExpression = QueryExpression.Select(
				new[] { QueryExpression.All() },
				from: QueryExpression.Table("TestTable"),
				where: QueryExpression.AreEqual(QueryExpression.Column("Column"), QueryExpression.Parameter("myParameter"))
				);
			var sql = TestHelpers.ConvertToSql(queryExpression);
			Assert.AreEqual("SELECT * FROM [TestTable] WHERE [Column] = @myParameter;", sql);
		}

		[TestMethod]
		public void Can_Write_Select_With_Value()
		{
			var queryExpression = QueryExpression.Select(
				new[] { QueryExpression.All() },
				from: QueryExpression.Table("TestTable"),
				where: QueryExpression.AreEqual(QueryExpression.Column("Column"), QueryExpression.Value("myValue"))
				);
			var sql = TestHelpers.ConvertToSql(queryExpression, out var parameters);
			var firstParameter = parameters.First();
			Assert.AreEqual("myValue", firstParameter.Value);
			Assert.AreEqual($"SELECT * FROM [TestTable] WHERE [Column] = @{firstParameter.Key};", sql);
		}

		[TestMethod]
		public void Can_Write_OrderBy_Query()
		{
			var queryExpression = QueryExpression.Select(
				new[] { QueryExpression.All() },
				from: QueryExpression.Table("TestTable"),
				orderBy: new[]
				{
					QueryExpression.OrderBy(QueryExpression.Column("Column1")),
					QueryExpression.OrderByDescending(QueryExpression.Column("Column2"))
				});
			var sql = TestHelpers.ConvertToSql(queryExpression);
			Assert.AreEqual("SELECT * FROM [TestTable] ORDER BY [Column1], [Column2] DESC;", sql);
		}

		[TestMethod]
		public void Can_Write_GroupBy_Query()
		{
			var queryExpression = QueryExpression.Select(
				new[] { QueryExpression.All() },
				from: QueryExpression.Table("TestTable"),
				groupBy: new[]
				{
					QueryExpression.GroupBy(QueryExpression.Column("Column1")),
					QueryExpression.GroupBy(QueryExpression.Column("Column2"))
				});
			var sql = TestHelpers.ConvertToSql(queryExpression);
			Assert.AreEqual("SELECT * FROM [TestTable] GROUP BY [Column1], [Column2];", sql);
		}

		[TestMethod]
		public void Can_Write_Limit_Query()
		{
			var queryExpression = QueryExpression.Select(
				new[] { QueryExpression.All() },
				from: QueryExpression.Table("TestTable"),
				limit: QueryExpression.Limit(QueryExpression.Value(1))
				);
			var sql = TestHelpers.ConvertToSql(queryExpression, out var parameters);
			var limitParameter = parameters.First();

			Assert.AreEqual(1, limitParameter.Value);
			Assert.AreEqual($"SELECT * FROM [TestTable] LIMIT @{limitParameter.Key};", sql);
		}

		[TestMethod]
		public void Can_Write_Limit_And_Offset_Query()
		{
			var queryExpression = QueryExpression.Select(
				new[] { QueryExpression.All() },
				from: QueryExpression.Table("TestTable"),
				limit: QueryExpression.Limit(QueryExpression.Value(1), QueryExpression.Value(2))
				);
			var sql = TestHelpers.ConvertToSql(queryExpression, out var parameters);
			var limitParameter = parameters.First();
			var offsetParameter = parameters.Skip(1).First();

			Assert.AreEqual(1, limitParameter.Value);
			Assert.AreEqual(2, offsetParameter.Value);
			Assert.AreEqual($"SELECT * FROM [TestTable] LIMIT @{limitParameter.Key} OFFSET @{offsetParameter.Key};", sql);
		}

		[TestMethod]
		public void Can_Write_Join_Query()
		{
			var leftTable = QueryExpression.Table("LeftTable");
			var rightTable = QueryExpression.Table("RightTable");
			var queryExpression = QueryExpression.Select(
				new[] { QueryExpression.All() },
				from: leftTable,
				joins: new[]
				{
					QueryExpression.Join(rightTable, QueryExpression.Column("RightId", leftTable), QueryExpression.Column("Id", rightTable))
				});
			var sql = TestHelpers.ConvertToSql(queryExpression);
			Assert.AreEqual("SELECT * FROM [LeftTable] INNER JOIN [RightTable] ON [LeftTable].[RightId] = [RightTable].[Id];", sql);
		}
	}
}
