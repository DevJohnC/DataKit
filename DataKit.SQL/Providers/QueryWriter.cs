using DataKit.SQL.QueryExpressions;
using System.Collections.Generic;
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

		protected void WriteIfNotNull(QueryExpression queryExpression, QueryExpressionVisitor queryExpressionVisitor)
		{
			if (queryExpression == null)
				return;
			queryExpressionVisitor.Visit(queryExpression);
		}

		protected void WriteExpressionCollection(IReadOnlyList<QueryExpression> expressions, QueryExpressionVisitor queryExpressionVisitor, string seperator = ", ", bool appendFinalSeparator = false)
		{
			var count = expressions.Count;
			for (var i = 0; i < count; i++)
			{
				WriteIfNotNull(expressions[i], queryExpressionVisitor);
				if (appendFinalSeparator || i < count - 1)
					queryText.Append(seperator);
			}
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

			WriteIfNotNull(selectStatementQueryExpression.From, queryExpressionVisitor);

			//  joins

			WriteIfNotNull(selectStatementQueryExpression.Where, queryExpressionVisitor);

			//  group by

			WriteIfNotNull(selectStatementQueryExpression.Having, queryExpressionVisitor);

			//  order by

			//  limit

			//  offset
		}

		public virtual void WriteProjection(
			ProjectionQueryExpression projectionQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor)
		{
			WriteExpressionCollection(projectionQueryExpression.Expressions, queryExpressionVisitor);
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

		public virtual void WriteAsOperator(
			AsOperatorQueryExpression asFunctionCallQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor
			)
		{
			queryExpressionVisitor.Visit(asFunctionCallQueryExpression.Expression);
			queryText.Append(" AS ");
			WriteIdentifier(asFunctionCallQueryExpression.Alias, queryExpressionVisitor);
		}

		public virtual void WriteIsInOperator(
			IsInOperatorQueryExpression isInFunctionCallQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor
			)
		{
			queryExpressionVisitor.Visit(isInFunctionCallQueryExpression.Expression);
			queryText.Append(" IN ");
			var expressionsAreQueries = isInFunctionCallQueryExpression.InExpressions[0] is ExecutableQueryExpression;
			if (!expressionsAreQueries)
				queryText.Append("(");

			WriteExpressionCollection(isInFunctionCallQueryExpression.InExpressions, queryExpressionVisitor);

			if (!expressionsAreQueries)
				queryText.Append(")");
			queryText.Append(" ");
		}

		public virtual void WriteCountFunction(
			CountFunctionCallQueryExpression countFunctionCallQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor)
		{
			queryText.Append(" COUNT(");
			WriteIfNotNull(countFunctionCallQueryExpression.Expression, queryExpressionVisitor);
			queryText.Append(") ");
		}
	}
}
