using DataKit.SQL.Providers;
using DataKit.SQL.QueryExpressions;
using DataKit.SQL.SqlServer.QueryExpressions;

namespace DataKit.SQL.SqlServer
{
	public class SqlServerQueryWriter : QueryWriter
	{
		public void Append(string str)
			=> queryText.Append(str);

		public override void WriteRandomFunction(RandomFunctionCallQueryExpression randomFunctionCallQueryExpression, QueryExpressionVisitor queryExpressionVisitor)
		{
			queryText.Append($" CAST(FLOOR(RAND() * {int.MaxValue}) AS BIGINT) ");
		}

		public override void WriteLastInsertedIdFunction(LastInsertedIdFunctionCallExpression lastInsertedIdFunctionCallExpression, QueryExpressionVisitor queryExpressionVisitor)
		{
			queryText.Append(" CAST(@@IDENTITY AS INT) ");
		}

		public override void WriteLimitExpressionParameter(LimitQueryExpression limitQueryExpression, QueryExpressionVisitor queryExpressionVisitor)
		{
			var currentQuery = statementStack.Peek() as SelectStatementQueryExpression;
			if (currentQuery.OrderBy == null)
				queryText.Append(" ORDER BY (SELECT NULL) ");

			if (limitQueryExpression.Offset == null)
			{
				queryText.Append(" OFFSET 0 ROWS FETCH FIRST ");
			}
			else
			{
				queryText.Append(" OFFSET ");
				WriteIfNotNull(limitQueryExpression.Offset, queryExpressionVisitor);
				queryText.Append(" ROWS FETCH NEXT ");
			}

			WriteIfNotNull(limitQueryExpression.Limit, queryExpressionVisitor);
			queryText.Append(" ROWS ONLY ");
		}

		public override void WriteExtension(QueryExpression queryExpression, QueryExpressionVisitor queryExpressionVisitor)
		{
			if (queryExpression is ISqlServerExtension sqlServerExtension)
				sqlServerExtension.Write(this, queryExpressionVisitor);
			base.WriteExtension(queryExpression, queryExpressionVisitor);
		}
	}
}
