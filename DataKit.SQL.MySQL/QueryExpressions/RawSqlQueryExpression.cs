using DataKit.SQL.QueryExpressions;
using System;

namespace DataKit.SQL.MySQL.QueryExpressions
{
	public class RawSqlQueryExpression : ExecutableExtensionQueryExpression, IMySQLExtension
	{
		public string Sql { get; }

		public override ExpressionType ExpressionType => ExpressionType.Extension;

		public RawSqlQueryExpression(string sql)
		{
			Sql = sql ?? throw new ArgumentNullException(nameof(sql));
		}

		void IMySQLExtension.Write(MySQLQueryWriter queryWriter, QueryExpressionVisitor queryExpressionVisitor)
		{
			queryWriter.Append(Sql);
		}
	}
}
