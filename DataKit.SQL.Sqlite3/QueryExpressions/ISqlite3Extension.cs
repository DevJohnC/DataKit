using DataKit.SQL.QueryExpressions;

namespace DataKit.SQL.Sqlite3.QueryExpressions
{
	public interface ISqlite3Extension
	{
		void Write(Sqlite3QueryWriter queryWriter, QueryExpressionVisitor queryExpressionVisitor);
	}
}
