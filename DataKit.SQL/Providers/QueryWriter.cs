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
			WriteIfNotNull(unaryOperatorQueryExpression.Expression, queryExpressionVisitor);
			switch (unaryOperatorQueryExpression.Operator)
			{
				case UnaryOperator.IsNull:
					queryText.Append(" IS NULL ");
					break;
				case UnaryOperator.IsNotNull:
					queryText.Append(" IS NOT NULL ");
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
