using DataKit.SQL.QueryExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataKit.SQL.UnitTests
{
	[TestClass]
	public class DeleteQueryGenerationTests
	{
		[TestMethod]
		public void Can_Write_Delete_Missing_Where()
		{
			var query = QueryExpression.Delete(QueryExpression.Table("TestTable"));
			var sql = TestHelpers.ConvertToSql(query);
			Assert.AreEqual("DELETE FROM [TestTable]", sql);
		}

		[TestMethod]
		public void Can_Write_Delete_With_Where()
		{
			var query = QueryExpression.Delete(
				QueryExpression.Table("TestTable"),
				QueryExpression.AreEqual(QueryExpression.Column("Id"), QueryExpression.Column("Id"))
				);
			var sql = TestHelpers.ConvertToSql(query);
			Assert.AreEqual("DELETE FROM [TestTable] WHERE [Id] = [Id]", sql);
		}
	}
}
