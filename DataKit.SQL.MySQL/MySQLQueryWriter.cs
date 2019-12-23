using DataKit.SQL.MySQL.QueryExpressions;
using DataKit.SQL.Providers;
using DataKit.SQL.QueryExpressions;

namespace DataKit.SQL.MySQL
{
	public class MySQLQueryWriter : QueryWriter
	{
		public void Append(string str)
			=> queryText.Append(str);

		public override void WriteIdentifier(IdentifierQueryExpression identifierQueryExpression, QueryExpressionVisitor queryExpressionVisitor)
		{
			if (identifierQueryExpression is NestedIdentifierQueryExpression nestedIdentifierQueryExpression &&
				nestedIdentifierQueryExpression.ParentIdentifier != null)
			{
				WriteIdentifier(nestedIdentifierQueryExpression.ParentIdentifier, queryExpressionVisitor);
				queryText.Append(".");
			}
			if (identifierQueryExpression.IdentifierName == "*")
				queryText.Append(" * ");
			else
				queryText.Append($" `{identifierQueryExpression.IdentifierName}` ");
		}

		public override void WriteRandomFunction(RandomFunctionCallQueryExpression randomFunctionCallQueryExpression, QueryExpressionVisitor queryExpressionVisitor)
		{
			queryText.Append($" CAST(FLOOR(RAND() * {int.MaxValue}) AS INT) ");
		}

		public override void WriteLastInsertedIdFunction(LastInsertedIdFunctionCallExpression lastInsertedIdFunctionCallExpression, QueryExpressionVisitor queryExpressionVisitor)
		{
			queryText.Append(" LAST_INSERT_ID() ");
		}

		public override void WriteExtension(QueryExpression queryExpression, QueryExpressionVisitor queryExpressionVisitor)
		{
			if (queryExpression is IMySQLExtension mysqlExtension)
				mysqlExtension.Write(this, queryExpressionVisitor);
			base.WriteExtension(queryExpression, queryExpressionVisitor);
		}
	}
}
