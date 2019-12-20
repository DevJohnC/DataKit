﻿using DataKit.SQL.Providers;
using DataKit.SQL.QueryExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DataKit.SQL.UnitTests
{
	[TestClass]
	public class SelectQueryGenerationTests
	{
		[TestMethod]
		public void Can_Write_Select_With_Projection_Only()
		{
			var queryExpression = QueryExpression.SelectStatement(
				new[] { QueryExpression.CountFunction() }
				);
			var sql = ConvertToSql(queryExpression);
			Assert.AreEqual("SELECT COUNT()", sql);
		}

		[TestMethod]
		public void Can_Write_Select_With_Multiple_Projections()
		{
			var queryExpression = QueryExpression.SelectStatement(
				new[] { QueryExpression.Column("Column1"), QueryExpression.Column("Column2") },
				from: QueryExpression.Table("TestTable")
				);
			var sql = ConvertToSql(queryExpression);
			Assert.AreEqual("SELECT [Column1], [Column2] FROM [TestTable]", sql);
		}

		[TestMethod]
		public void Can_Write_Select_With_Alias()
		{
			var queryExpression = QueryExpression.SelectStatement(
				new[] { QueryExpression.As(QueryExpression.Column("TestColumn"), "aliasTest", out var _) },
				from: QueryExpression.Table("TestTable")
				);
			var sql = ConvertToSql(queryExpression);
			Assert.AreEqual("SELECT [TestColumn] AS [aliasTest] FROM [TestTable]", sql);
		}

		[TestMethod]
		public void Can_Write_Select_With_In_Query()
		{
			var queryExpression = QueryExpression.SelectStatement(
				new[] { QueryExpression.IsIn(
					QueryExpression.Column("Column"),
					QueryExpression.SelectStatement(new[] { QueryExpression.Column("Id") }, from: QueryExpression.Table("OtherTable"))
					) },
				from: QueryExpression.Table("TestTable")
				);
			var sql = ConvertToSql(queryExpression);
			Assert.AreEqual("SELECT [Column] IN SELECT [Id] FROM [OtherTable] FROM [TestTable]", sql);
		}

		[TestMethod]
		public void Can_Write_Select_With_Nested_Conditions()
		{
			var queryExpression = QueryExpression.SelectStatement(
				new[] { QueryExpression.All() },
				from: QueryExpression.Table("TestTable"),
				where: QueryExpression.AndAlso(
					QueryExpression.AreEqual(QueryExpression.Column("Left"), QueryExpression.Column("Right")),
					QueryExpression.OrElse(
						QueryExpression.AreEqual(QueryExpression.Column("Left"), QueryExpression.Column("Right")),
						QueryExpression.AreNotEqual(QueryExpression.Column("Left"), QueryExpression.Column("Right"))
					))
				);
			var sql = ConvertToSql(queryExpression);
			Assert.AreEqual("SELECT * FROM [TestTable] WHERE ([Left] = [Right] AND ([Left] = [Right] OR [Left] != [Right]))", sql);
		}

		[TestMethod]
		public void Can_Write_Select_With_Parameter_Reference()
		{
			var queryExpression = QueryExpression.SelectStatement(
				new[] { QueryExpression.All() },
				from: QueryExpression.Table("TestTable"),
				where: QueryExpression.AreEqual(QueryExpression.Column("Column"), QueryExpression.Parameter("myParameter"))
				);
			var sql = ConvertToSql(queryExpression);
			Assert.AreEqual("SELECT * FROM [TestTable] WHERE [Column] = @myParameter", sql);
		}

		[TestMethod]
		public void Can_Write_Select_With_Value()
		{
			var queryExpression = QueryExpression.SelectStatement(
				new[] { QueryExpression.All() },
				from: QueryExpression.Table("TestTable"),
				where: QueryExpression.AreEqual(QueryExpression.Column("Column"), QueryExpression.Value("myValue"))
				);
			var sql = ConvertToSql(queryExpression, out var parameters);
			var firstParameter = parameters.First();
			Assert.AreEqual("myValue", firstParameter.Value);
			Assert.AreEqual($"SELECT * FROM [TestTable] WHERE [Column] = @{firstParameter.Key}", sql);
		}

		private string ConvertToSql(QueryExpression queryExpression)
		{
			return ConvertToSql(queryExpression, out var _);
		}

		private string ConvertToSql(QueryExpression queryExpression, out ParameterBag valueParameters)
		{
			var queryWriter = new QueryWriter();
			var converter = new QueryConverter();
			converter.WriteQuery(queryWriter, queryExpression);
			valueParameters = queryWriter.Parameters;
			return queryWriter.ToString().ReduceWhitespace().Trim();
		}
	}
}
