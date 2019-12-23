using DataKit.SQL.MySQL.QueryExpressions;

namespace DataKit.SQL.QueryExpressions
{
	public static class MySQLQueryExpression
	{
		public static RawSqlQueryExpression Raw(string sql)
			=> new RawSqlQueryExpression(sql);
	}
}
