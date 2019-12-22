using DataKit.SQL.QueryExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataKit.SQL.UnitTests
{
	[TestClass]
	public class FunctionQueryGenerationTests
	{
		[TestMethod]
		public void Can_Write_Random_Function()
		{
			var queryExpression = QueryExpression.Select(
				new[] { QueryExpression.Random() }
				);
			var sql = TestHelpers.ConvertToSql(queryExpression);
			Assert.AreEqual("SELECT RANDOM();", sql);
		}

		[TestMethod]
		public void Can_Write_Min_Function_Without_Args()
		{
			var queryExpression = QueryExpression.Select(
				new[] { QueryExpression.Min() }
				);
			var sql = TestHelpers.ConvertToSql(queryExpression);
			Assert.AreEqual("SELECT MIN();", sql);
		}

		[TestMethod]
		public void Can_Write_Min_Function_With_Args()
		{
			var queryExpression = QueryExpression.Select(
				new[] { QueryExpression.Min(QueryExpression.Column("Id")) }
				);
			var sql = TestHelpers.ConvertToSql(queryExpression);
			Assert.AreEqual("SELECT MIN([Id]);", sql);
		}

		[TestMethod]
		public void Can_Write_Max_Function_Without_Args()
		{
			var queryExpression = QueryExpression.Select(
				new[] { QueryExpression.Max() }
				);
			var sql = TestHelpers.ConvertToSql(queryExpression);
			Assert.AreEqual("SELECT MAX();", sql);
		}

		[TestMethod]
		public void Can_Write_Max_Function_With_Args()
		{
			var queryExpression = QueryExpression.Select(
				new[] { QueryExpression.Max(QueryExpression.Column("Id")) }
				);
			var sql = TestHelpers.ConvertToSql(queryExpression);
			Assert.AreEqual("SELECT MAX([Id]);", sql);
		}

		[TestMethod]
		public void Can_Write_Concat_Function()
		{
			var queryExpression = QueryExpression.Select(
				new[] { QueryExpression.Concat(QueryExpression.Column("Column1"), QueryExpression.Column("Column2")) }
				);
			var sql = TestHelpers.ConvertToSql(queryExpression);
			Assert.AreEqual("SELECT CONCAT([Column1], [Column2]);", sql);
		}
	}
}
