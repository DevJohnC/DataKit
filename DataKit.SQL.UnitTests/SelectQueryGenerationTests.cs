using DataKit.SQL.Providers;
using DataKit.SQL.QueryExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

		private string ConvertToSql(QueryExpression queryExpression)
		{
			var queryWriter = new QueryWriter();
			var converter = new QueryConverter();
			converter.WriteQuery(queryWriter, queryExpression);
			return queryWriter.ToString().ReduceWhitespace().Trim();
		}
	}
}
