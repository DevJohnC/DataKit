using System;
using DataKit.SQL.QueryExpressions;

namespace DataKit.SQL.Sqlite3.QueryExpressions
{
	public class RawSqlQueryExpression : ExecutableExtensionQueryExpression, ISqlite3Extension
	{
		public string Sql { get; }

		public override ExpressionType ExpressionType => ExpressionType.Extension;

		public RawSqlQueryExpression(string sql)
		{
			Sql = sql ?? throw new ArgumentNullException(nameof(sql));
		}

		void ISqlite3Extension.Write(Sqlite3QueryWriter queryWriter, QueryExpressionVisitor queryExpressionVisitor)
		{
			queryWriter.Append(Sql);
		}
	}
}
