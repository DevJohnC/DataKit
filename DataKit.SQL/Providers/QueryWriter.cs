using DataKit.SQL.QueryExpressions;
using System.Text;

namespace DataKit.SQL.Providers
{
	public class QueryWriter
	{
		protected readonly StringBuilder queryText = new StringBuilder();

		public ParameterBag Parameters { get; } = new ParameterBag();

		public override string ToString()
		{
			return queryText.ToString();
		}

		protected void VisitIfNotNull(QueryExpression queryExpression, QueryExpressionVisitor queryExpressionVisitor)
		{
			if (queryExpression == null)
				return;
			queryExpressionVisitor.Visit(queryExpression);
		}

		public virtual void WriteExtension(QueryExpression queryExpression,
			QueryExpressionVisitor queryExpressionVisitor)
		{
		}

		public virtual void WriteIdentifier(IdentifierQueryExpression identifierQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor)
		{
			if (identifierQueryExpression is NestedIdentifierQueryExpression nestedIdentifierQueryExpression &&
				nestedIdentifierQueryExpression.ParentIdentifier != null)
			{
				WriteIdentifier(nestedIdentifierQueryExpression.ParentIdentifier, queryExpressionVisitor);
				queryText.Append(".");
			}
			queryText.Append($" [{identifierQueryExpression.IdentifierName}] ");
		}

		public virtual void WriteSelectStatement(
			SelectStatementQueryExpression selectStatementQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor)
		{
			queryText.Append("SELECT ");
			if (selectStatementQueryExpression.Projection == null || selectStatementQueryExpression.Projection.Expressions == null ||
				selectStatementQueryExpression.Projection.Expressions.Length < 1)
				throw new ProjectionMissingException("SELECT query must include a projection.");
			queryExpressionVisitor.Visit(selectStatementQueryExpression.Projection);

			VisitIfNotNull(selectStatementQueryExpression.From, queryExpressionVisitor);

			//  joins

			VisitIfNotNull(selectStatementQueryExpression.Where, queryExpressionVisitor);

			//  group by

			VisitIfNotNull(selectStatementQueryExpression.Having, queryExpressionVisitor);

			//  order by

			//  limit

			//  offset
		}

		public virtual void WriteProjection(
			ProjectionQueryExpression projectionQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor)
		{
			var count = projectionQueryExpression.Expressions.Length;
			var i = 0;
			foreach (var expression in projectionQueryExpression.Expressions)
			{
				i++;
				queryExpressionVisitor.Visit(expression);
				if (i < count)
					queryText.Append(", ");
			}
		}

		public virtual void WriteFromQueryParameter(
			FromQueryExpression fromQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor)
		{
			queryText.Append(" FROM ");
			queryExpressionVisitor.Visit(fromQueryExpression.From);
		}

		public virtual void WriteWhereQueryParameters(
			WhereQueryExpression whereQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor)
		{
			queryText.Append(" WHERE ");
			queryExpressionVisitor.Visit(whereQueryExpression.Where);
		}

		public virtual void WriteHavingQueryParameters(
			HavingQueryExpression whereQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor)
		{
			queryText.Append(" HAVING ");
			queryExpressionVisitor.Visit(whereQueryExpression.Having);
		}

		public virtual void WriteCountFunction(
			CountFunctionCallQueryExpression countFunctionCallQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor)
		{
			queryText.Append(" COUNT(");
			VisitIfNotNull(countFunctionCallQueryExpression.Expression, queryExpressionVisitor);
			queryText.Append(") ");
		}
	}
}
