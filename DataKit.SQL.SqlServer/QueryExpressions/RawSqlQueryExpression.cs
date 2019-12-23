using DataKit.SQL.QueryExpressions;
using System;

namespace DataKit.SQL.SqlServer.QueryExpressions
{
	public class RawSqlQueryExpression : ExecutableExtensionQueryExpression, ISqlServerExtension
	{
		public string Sql { get; }

		public override ExpressionType ExpressionType => ExpressionType.Extension;

		public RawSqlQueryExpression(string sql)
		{
			Sql = sql ?? throw new ArgumentNullException(nameof(sql));
		}

		void ISqlServerExtension.Write(SqlServerQueryWriter queryWriter, QueryExpressionVisitor queryExpressionVisitor)
		{
			queryWriter.Append(Sql);
		}
	}
}
