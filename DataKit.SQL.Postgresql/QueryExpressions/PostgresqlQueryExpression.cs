using DataKit.SQL.Postgresql.QueryExpressions;

namespace DataKit.SQL.QueryExpressions
{
	public static class PostgresqlQueryExpression
	{
		public static RawSqlQueryExpression Raw(string sql)
			=> new RawSqlQueryExpression(sql);
	}
}
