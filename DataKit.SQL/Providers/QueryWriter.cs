using DataKit.SQL.QueryExpressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataKit.SQL.Providers
{
	public class QueryWriter
	{
		protected readonly StringBuilder queryText = new StringBuilder();

		protected readonly Stack<ExecutableQueryExpression> statementStack = new Stack<ExecutableQueryExpression>();

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

		protected bool IsSubQuery()
		{
			return statementStack.Count > 1;
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

		public virtual void WriteMultipleStatements(
			MultipleStatementQueryExpression multipleStatementQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor
			)
		{
			WriteExpressionCollection(multipleStatementQueryExpression.Queries, queryExpressionVisitor, seperator: " ");
		}

		public virtual void WriteSelectStatement(
			SelectStatementQueryExpression selectStatementQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor)
		{
			statementStack.Push(selectStatementQueryExpression);

			if (IsSubQuery())
				queryText.Append(" ( ");

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

			if (IsSubQuery())
				queryText.Append(" ) ");
			else
				queryText.Append("; ");

			statementStack.Pop();
		}

		public virtual void WriteInsertStatement(
			InsertStatementQueryExpression insertStatementQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor
			)
		{
			statementStack.Push(insertStatementQueryExpression);

			if (IsSubQuery())
				queryText.Append(" ( ");

			queryText.Append("INSERT ");
			WriteIfNotNull(insertStatementQueryExpression.Into, queryExpressionVisitor);

			queryText.Append(" ( ");
			WriteExpressionCollection(insertStatementQueryExpression.Columns, queryExpressionVisitor);
			queryText.Append(" ) ");
			queryText.Append(" VALUES ");

			for(var i = 0; i < insertStatementQueryExpression.RowsExpressions.Length; i++)
			{
				queryText.Append(" ( ");
				WriteExpressionCollection(insertStatementQueryExpression.RowsExpressions[i], queryExpressionVisitor);
				queryText.Append(" ) ");
				if (i < insertStatementQueryExpression.RowsExpressions.Length - 1)
					queryText.Append(", ");
			}

			if (IsSubQuery())
				queryText.Append(" ) ");
			else
				queryText.Append("; ");

			statementStack.Pop();
		}

		public virtual void WriteUpdateStatement(
			UpdateStatementQueryExpression updateStatementQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor
			)
		{
			statementStack.Push(updateStatementQueryExpression);

			if (IsSubQuery())
				queryText.Append(" ( ");

			queryText.Append("UPDATE ");
			WriteIfNotNull(updateStatementQueryExpression.Into, queryExpressionVisitor);
			queryText.Append(" SET ");
			for (var i = 0; i < updateStatementQueryExpression.Columns.Length; i++)
			{
				WriteIfNotNull(updateStatementQueryExpression.Columns[i], queryExpressionVisitor);
				queryText.Append(" = ");
				WriteIfNotNull(updateStatementQueryExpression.ValueExpressions[i], queryExpressionVisitor);

				if (i < updateStatementQueryExpression.Columns.Length - 1)
					queryText.Append(", ");
			}
			WriteIfNotNull(updateStatementQueryExpression.Where, queryExpressionVisitor);

			if (IsSubQuery())
				queryText.Append(" ) ");
			else
				queryText.Append("; ");

			statementStack.Pop();
		}

		public virtual void WriteDeleteStatement(
			DeleteStatementQueryExpression deleteStatementQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor
			)
		{
			statementStack.Push(deleteStatementQueryExpression);

			if (IsSubQuery())
				queryText.Append(" ( ");

			queryText.Append("DELETE ");
			WriteIfNotNull(deleteStatementQueryExpression.From, queryExpressionVisitor);
			WriteIfNotNull(deleteStatementQueryExpression.Where, queryExpressionVisitor);

			if (IsSubQuery())
				queryText.Append(" ) ");
			else
				queryText.Append("; ");

			statementStack.Pop();
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

		public virtual void WriteIntoQueryParameter(
			IntoQueryExpression intoQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor
			)
		{
			queryText.Append(" INTO ");
			queryExpressionVisitor.Visit(intoQueryExpression.Expression);
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

		public virtual void WriteRandomFunction(
			RandomFunctionCallQueryExpression randomFunctionCallQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor)
		{
			queryText.Append(" RANDOM() ");
		}

		public virtual void WriteMinFunction(
			MinFunctionCallQueryExpression minFunctionCallQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor)
		{
			queryText.Append(" MIN(");
			WriteIfNotNull(minFunctionCallQueryExpression.Expression, queryExpressionVisitor);
			queryText.Append(") ");
		}

		public virtual void WriteMaxFunction(
			MaxFunctionCallQueryExpression maxFunctionCallQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor)
		{
			queryText.Append(" MAX(");
			WriteIfNotNull(maxFunctionCallQueryExpression.Expression, queryExpressionVisitor);
			queryText.Append(") ");
		}

		public virtual void WriteConcatFunction(
			ConcatFunctionCallQueryExpression concatFunctionCallQueryExpression,
			QueryExpressionVisitor queryExpressionVisitor
			)
		{
			queryText.Append(" CONCAT(");
			WriteExpressionCollection(concatFunctionCallQueryExpression.Expressions, queryExpressionVisitor);
			queryText.Append(") ");
		}

		public virtual void WriteLastInsertedIdFunction(
			LastInsertedIdFunctionCallExpression lastInsertedIdFunctionCallExpression,
			QueryExpressionVisitor queryExpressionVisitor
			)
		{
			//  todo: decide what to do about this
			//  last insert id is literally different on every single platform
			//  the obvious thing to do would be to make QueryWriter abstract
			//  but that means writing an implementation of a writer for testing
			//  purposes and use in other contexts I can't think of now
			throw new NotImplementedException();
		}
	}
}
