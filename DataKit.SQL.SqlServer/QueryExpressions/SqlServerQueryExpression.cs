using DataKit.SQL.SqlServer.QueryExpressions;

namespace DataKit.SQL.QueryExpressions
{
	public static class SqlServerQueryExpression
	{
		public static RawSqlQueryExpression Raw(string sql)
			=> new RawSqlQueryExpression(sql);
	}
}
