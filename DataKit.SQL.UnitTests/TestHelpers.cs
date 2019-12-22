using DataKit.SQL.Providers;
using DataKit.SQL.QueryExpressions;

namespace DataKit.SQL.UnitTests
{
	public static class TestHelpers
	{
		public static string ConvertToSql(QueryExpression queryExpression)
		{
			return ConvertToSql(queryExpression, out var _);
		}

		public static string ConvertToSql(QueryExpression queryExpression, out ParameterBag valueParameters)
		{
			var queryWriter = new QueryWriter();
			var converter = new QueryConverter();
			converter.WriteQuery(queryWriter, queryExpression);
			valueParameters = queryWriter.Parameters;
			return queryWriter.ToString().ReduceWhitespace().Trim();
		}
	}
}
