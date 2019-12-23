using DataKit.SQL.QueryExpressions;
using System;

namespace DataKit.SQL.Postgresql.QueryExpressions
{
	public class RawSqlQueryExpression : ExecutableExtensionQueryExpression, IPostgresqlExtension
	{
		public string Sql { get; }

		public override ExpressionType ExpressionType => ExpressionType.Extension;

		public RawSqlQueryExpression(string sql)
		{
			Sql = sql ?? throw new ArgumentNullException(nameof(sql));
		}

		void IPostgresqlExtension.Write(PostgresqlQueryWriter queryWriter, QueryExpressionVisitor queryExpressionVisitor)
		{
			queryWriter.Append(Sql);
		}
	}
}
