using DataKit.SQL.QueryExpressions;

namespace DataKit.SQL.SqlServer.QueryExpressions
{
	public interface ISqlServerExtension
	{
		void Write(SqlServerQueryWriter queryWriter, QueryExpressionVisitor queryExpressionVisitor);
	}
}
