using DataKit.SQL.QueryExpressions;

namespace DataKit.SQL.MySQL.QueryExpressions
{
	public interface IMySQLExtension
	{
		void Write(MySQLQueryWriter queryWriter, QueryExpressionVisitor queryExpressionVisitor);
	}
}
