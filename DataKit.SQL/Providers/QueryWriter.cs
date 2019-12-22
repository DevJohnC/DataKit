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

		public virtual void WriteParameterName(string parameterName)
		{
			queryText.Append($" @{parameterName} ");
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
			if (identifierQueryExpression.IdentifierName == "*")
				queryText.Append(" * ");
			else
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

			if (selectStatementQueryExpression.Joins != null && selectStatementQueryExpression.Joins.Length > 0)
				WriteExpressionCollection(selectStatementQueryExpression.Joins, queryExpressionVisitor, seperator: " ");

			WriteIfNotNull(selectStatementQueryExpression.Where, queryExpressionVisitor);

			WriteIfNotNull(selectStatementQueryExpression.GroupBy, queryExpressionVisitor);

			WriteIfNotNull(selectStatementQueryExpression.Having, queryExpressionVisitor);

			WriteIfNotNull(selectStatementQueryExpression.OrderBy, queryExpressionVisitor);

			WriteIfNotNull(selectStatementQueryExpression.Limit, queryExpressionVisitor);
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
			queryExpressionVisitor.Visit(fromQueryExpression.Expression);
		}

		public virtual void WriteJoinQueryParameter(
			JoinQueryExpression joinQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor
			)
		{
			switch (joinQueryExpression.Direction)
			{
				case JoinDirection.Inner:
					queryText.Append(" INNER ");
					break;
				case JoinDirection.Left:
					queryText.Append(" LEFT OUTER ");
					break;
				case JoinDirection.Right:
					queryText.Append(" RIGHT OUTER ");
					break;
			}
			queryText.Append(" JOIN ");
			WriteIfNotNull(joinQueryExpression.TargetExpression, queryExpressionVisitor);
			queryText.Append(" ON ");
			WriteIfNotNull(joinQueryExpression.OnExpression, queryExpressionVisitor);
		}

		public virtual void WriteWhereQueryParameters(
			WhereQueryExpression whereQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor)
		{
			queryText.Append(" WHERE ");
			queryExpressionVisitor.Visit(whereQueryExpression.Expression);
		}

		public virtual void WriteHavingQueryParameters(
			HavingQueryExpression whereQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor)
		{
			queryText.Append(" HAVING ");
			queryExpressionVisitor.Visit(whereQueryExpression.Expression);
		}

		public virtual void WriteOrderByQueryParameter(
			OrderByCollectionQueryExpression orderByCollectionQuery,
			QueryExpressionVisitor queryExpressionVisitor
			)
		{
			queryText.Append(" ORDER BY ");
			WriteExpressionCollection(orderByCollectionQuery.Expressions, queryExpressionVisitor);
		}

		public virtual void WriteOrderByExpressionQueryParameter(
			OrderByQueryExpression orderByQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor
			)
		{
			WriteIfNotNull(orderByQueryExpression.Expression, queryExpressionVisitor);
			if (orderByQueryExpression.Direction == OrderByDirection.Descending)
				queryText.Append(" DESC ");
		}

		public virtual void WriteGroupByQueryParameter(
			GroupByCollectionQueryExpression groupByCollectionQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor
			)
		{
			queryText.Append(" GROUP BY ");
			WriteExpressionCollection(groupByCollectionQueryExpression.Expressions, queryExpressionVisitor);
		}

		public virtual void WriteGroupByExpressionQueryParameter(
			GroupByQueryExpression groupByQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor
			)
		{
			WriteIfNotNull(groupByQueryExpression.Expression, queryExpressionVisitor);
		}

		public virtual void WriteLimitExpressionParameter(
			LimitQueryExpression limitQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor
			)
		{
			queryText.Append(" LIMIT ");
			WriteIfNotNull(limitQueryExpression.Limit, queryExpressionVisitor);

			if (limitQueryExpression.Offset != null)
			{
				queryText.Append(" OFFSET ");
				WriteIfNotNull(limitQueryExpression.Offset, queryExpressionVisitor);
			}
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
			if (isInFunctionCallQueryExpression.IsNotIn)
				queryText.Append("NOT ");
			var expressionsAreQueries = isInFunctionCallQueryExpression.InExpressions[0] is ExecutableQueryExpression;
			if (!expressionsAreQueries)
				queryText.Append("(");

			WriteExpressionCollection(isInFunctionCallQueryExpression.InExpressions, queryExpressionVisitor);

			if (!expressionsAreQueries)
				queryText.Append(")");
			queryText.Append(" ");
		}

		public virtual void WriteUnaryOperator(
			UnaryOperatorQueryExpression unaryOperatorQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor
			)
		{
			switch (unaryOperatorQueryExpression.Operator)
			{
				case UnaryOperator.IsNull:
					WriteIfNotNull(unaryOperatorQueryExpression.Expression, queryExpressionVisitor);
					queryText.Append(" IS NULL ");
					break;
				case UnaryOperator.IsNotNull:
					WriteIfNotNull(unaryOperatorQueryExpression.Expression, queryExpressionVisitor);
					queryText.Append(" IS NOT NULL ");
					break;
				case UnaryOperator.Distinct:
					queryText.Append(" DISTINCT ");
					WriteIfNotNull(unaryOperatorQueryExpression.Expression, queryExpressionVisitor);
					break;
			}
		}

		public virtual void WriteBinaryOperator(
			BinaryOperatorQueryExpression binaryOperatorQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor
			)
		{
			if (binaryOperatorQueryExpression.Operator == BinaryOperator.AndAlso ||
				binaryOperatorQueryExpression.Operator == BinaryOperator.OrElse)
				queryText.Append(" (");

			WriteIfNotNull(binaryOperatorQueryExpression.Left, queryExpressionVisitor);

			switch (binaryOperatorQueryExpression.Operator)
			{
				case BinaryOperator.Addition:
					queryText.Append(" + ");
					break;
				case BinaryOperator.Subtraction:
					queryText.Append(" - ");
					break;
				case BinaryOperator.Division:
					queryText.Append(" / ");
					break;
				case BinaryOperator.Multiplication:
					queryText.Append(" * ");
					break;
				case BinaryOperator.Modulus:
					queryText.Append(" % ");
					break;

				case BinaryOperator.AndAlso:
					queryText.Append(" AND ");
					break;
				case BinaryOperator.OrElse:
					queryText.Append(" OR ");
					break;

				case BinaryOperator.AreEqual:
					queryText.Append(" = ");
					break;
				case BinaryOperator.AreNotEqual:
					queryText.Append(" != ");
					break;
				case BinaryOperator.GreaterThan:
					queryText.Append(" > ");
					break;
				case BinaryOperator.GreaterThanOrEqualTo:
					queryText.Append(" >= ");
					break;
				case BinaryOperator.LessThan:
					queryText.Append(" < ");
					break;
				case BinaryOperator.LessThanOrEqualTo:
					queryText.Append(" <= ");
					break;
				case BinaryOperator.Like:
					queryText.Append(" LIKE ");
					break;
				case BinaryOperator.NotLike:
					queryText.Append(" NOT LIKE ");
					break;

				case BinaryOperator.BitwiseAnd:
					queryText.Append(" & ");
					break;
				case BinaryOperator.BitwiseOr:
					queryText.Append(" | ");
					break;
				case BinaryOperator.BitwiseExclusiveOr:
					queryText.Append(" ^ ");
					break;
			}

			WriteIfNotNull(binaryOperatorQueryExpression.Right, queryExpressionVisitor);

			if (binaryOperatorQueryExpression.Operator == BinaryOperator.AndAlso ||
				binaryOperatorQueryExpression.Operator == BinaryOperator.OrElse)
				queryText.Append(") ");
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
