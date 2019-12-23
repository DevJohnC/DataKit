using DataKit.SQL.QueryExpressions;

namespace DataKit.SQL.Postgresql.QueryExpressions
{
	public interface IPostgresqlExtension
	{
		void Write(PostgresqlQueryWriter queryWriter, QueryExpressionVisitor queryExpressionVisitor);
	}
}
