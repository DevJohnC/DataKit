using DataKit.SQL.QueryExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DataKit.SQL.UnitTests
{
	[TestClass]
	public class UpdateQueryGenerationTests
	{
		[TestMethod]
		public void Can_Write_Update_With_Values()
		{
			var query = QueryExpression.Update(
				QueryExpression.Table("TestTable"),
				new[] { QueryExpression.Column("Value1"), QueryExpression.Column("Value2") },
				new[] { QueryExpression.Value(1), QueryExpression.Value(2) }
				);
			var sql = TestHelpers.ConvertToSql(query, out var parameters);

			var value1Parameter = parameters.First();
			var value2Parameter = parameters.Skip(1).First();

			Assert.AreEqual(1, value1Parameter.Value);
			Assert.AreEqual(2, value2Parameter.Value);
			Assert.AreEqual(sql, $"UPDATE [TestTable] SET [Value1] = @{value1Parameter.Key}, [Value2] = @{value2Parameter.Key}");
		}

		[TestMethod]
		public void Can_Write_Update_With_Expressions()
		{
			var query = QueryExpression.Update(
				QueryExpression.Table("TestTable"),
				new[] { QueryExpression.Column("Value1")},
				new[] { QueryExpression.Add(QueryExpression.Column("Value2"), QueryExpression.Value(1)) }
				);
			var sql = TestHelpers.ConvertToSql(query, out var parameters);

			var value1Parameter = parameters.First();

			Assert.AreEqual(1, value1Parameter.Value);
			Assert.AreEqual(sql, $"UPDATE [TestTable] SET [Value1] = [Value2] + @{value1Parameter.Key}");
		}

		[TestMethod]
		public void Can_Write_Update_With_Where()
		{
			var query = QueryExpression.Update(
				QueryExpression.Table("TestTable"),
				new[] { QueryExpression.Column("Value1") },
				new[] { QueryExpression.Column("Value2") },
				QueryExpression.AreEqual(QueryExpression.Column("Value2"), QueryExpression.Column("Value3"))
				);
			var sql = TestHelpers.ConvertToSql(query);
			Assert.AreEqual(sql, $"UPDATE [TestTable] SET [Value1] = [Value2] WHERE [Value2] = [Value3]");
		}
	}
}
