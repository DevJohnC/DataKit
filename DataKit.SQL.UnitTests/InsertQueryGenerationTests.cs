using DataKit.SQL.QueryExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DataKit.SQL.UnitTests
{
	[TestClass]
	public class InsertQueryGenerationTests
	{
		[TestMethod]
		public void Can_Write_Insert()
		{
			var query = QueryExpression.Insert(
				QueryExpression.Table("TestTable"),
				new[] { QueryExpression.Column("Value1"), QueryExpression.Column("Value2") },
				new[] { QueryExpression.Value(1), QueryExpression.Value(2) },
				new[] { QueryExpression.Value(3), QueryExpression.Value(4) }
				);
			var sql = TestHelpers.ConvertToSql(query, out var parameters);

			var value1Parameter = parameters.First();
			var value2Parameter = parameters.Skip(1).First();
			var value3Parameter = parameters.Skip(2).First();
			var value4Parameter = parameters.Skip(3).First();

			Assert.AreEqual(1, value1Parameter.Value);
			Assert.AreEqual(2, value2Parameter.Value);
			Assert.AreEqual(3, value3Parameter.Value);
			Assert.AreEqual(4, value4Parameter.Value);
			Assert.AreEqual(sql, $"INSERT INTO [TestTable] ([Value1], [Value2]) VALUES (@{value1Parameter.Key}, @{value2Parameter.Key}), (@{value3Parameter.Key}, @{value4Parameter.Key})");
		}
	}
}
