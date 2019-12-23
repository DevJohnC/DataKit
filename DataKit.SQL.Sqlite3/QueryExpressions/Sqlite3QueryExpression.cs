using DataKit.SQL.Sqlite3.QueryExpressions;

namespace DataKit.SQL.QueryExpressions
{
	public static class Sqlite3QueryExpression
	{
		public static RawSqlQueryExpression Raw(string sql)
			=> new RawSqlQueryExpression(sql);
	}
}
