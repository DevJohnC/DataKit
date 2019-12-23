using DataKit.SQL.Providers;
using DataKit.SQL.QueryExpressions;

namespace DataKit.SQL.Postgresql
{
	public class PostgresqlQueryWriter : QueryWriter
	{
		public void Append(string str)
			=> queryText.Append(str);

		public override void WriteRandomFunction(RandomFunctionCallQueryExpression randomFunctionCallQueryExpression, QueryExpressionVisitor queryExpressionVisitor)
		{
			queryText.Append($" FLOOR(RANDOM() * {int.MaxValue})::int ");
		}

		public override void WriteLastInsertedIdFunction(LastInsertedIdFunctionCallExpression lastInsertedIdFunctionCallExpression, QueryExpressionVisitor queryExpressionVisitor)
		{
			queryText.Append(" LASTVAL() ");
		}

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
				queryText.Append($" \"{identifierQueryExpression.IdentifierName}\" ");
		}
	}
}
