using DataKit.SQL.QueryExpressions;

namespace DataKit.SQL.Providers
{
	public class QueryConverter
	{
		public virtual void WriteQuery(QueryWriter queryWriter, QueryExpression queryExpression)
		{
			var visitor = new ConvertingVisitor(queryWriter);
			visitor.Visit(queryExpression);
		}

		private class ConvertingVisitor : QueryExpressionVisitor
		{
			private readonly QueryWriter _writer;

			public ConvertingVisitor(QueryWriter writer)
			{
				_writer = writer;
			}

			public override QueryExpression VisitStatement(StatementQueryExpression statementQueryExpression)
			{
				switch (statementQueryExpression)
				{
					case SelectStatementQueryExpression select:
						_writer.WriteSelectStatement(select, this);
						break;
					case DeleteStatementQueryExpression delete:
						_writer.WriteDeleteStatement(delete, this);
						break;
				}
				return base.VisitStatement(statementQueryExpression);
			}

			public override QueryExpression VisitProjection(ProjectionQueryExpression projectionQueryExpression)
			{
				_writer.WriteProjection(projectionQueryExpression, this);
				return base.VisitProjection(projectionQueryExpression);
			}

			public override QueryExpression VisitQueryParameter(QueryParameterQueryExpression queryParameterQueryExpression)
			{
				switch (queryParameterQueryExpression)
				{
					case FromQueryExpression from:
						_writer.WriteFromQueryParameter(from, this);
						break;
					case JoinQueryExpression join:
						_writer.WriteJoinQueryParameter(join, this);
						break;
					case WhereQueryExpression where:
						_writer.WriteWhereQueryParameters(where, this);
						break;
					case HavingQueryExpression having:
						_writer.WriteHavingQueryParameters(having, this);
						break;
					case OrderByCollectionQueryExpression orderByCollection:
						_writer.WriteOrderByQueryParameter(orderByCollection, this);
						break;
					case OrderByQueryExpression orderBy:
						_writer.WriteOrderByExpressionQueryParameter(orderBy, this);
						break;
					case GroupByCollectionQueryExpression groupByCollection:
						_writer.WriteGroupByQueryParameter(groupByCollection, this);
						break;
					case GroupByQueryExpression groupBy:
						_writer.WriteGroupByExpressionQueryParameter(groupBy, this);
						break;
					case LimitQueryExpression limit:
						_writer.WriteLimitExpressionParameter(limit, this);
						break;
				}
				return base.VisitQueryParameter(queryParameterQueryExpression);
			}

			public override QueryExpression VisitIdentifier(IdentifierQueryExpression identifierQueryExpression)
			{
				_writer.WriteIdentifier(identifierQueryExpression, this);
				return base.VisitIdentifier(identifierQueryExpression);
			}

			public override QueryExpression VisitFunctionCall(FunctionCallQueryExpression functionCallQueryExpression)
			{
				switch (functionCallQueryExpression)
				{
					case CountFunctionCallQueryExpression count:
						_writer.WriteCountFunction(count, this);
						break;
				}
				return base.VisitFunctionCall(functionCallQueryExpression);
			}

			public override QueryExpression VisitOperator(OperatorQueryExpression operatorQueryExpression)
			{
				switch (operatorQueryExpression)
				{
					case AsOperatorQueryExpression @as:
						_writer.WriteAsOperator(@as, this);
						break;
					case IsInOperatorQueryExpression isIn:
						_writer.WriteIsInOperator(isIn, this);
						break;
					case UnaryOperatorQueryExpression unary:
						_writer.WriteUnaryOperator(unary, this);
						break;
					case BinaryOperatorQueryExpression binary:
						_writer.WriteBinaryOperator(binary, this);
						break;
				}
				return base.VisitOperator(operatorQueryExpression);
			}

			public override QueryExpression VisitParameter(ParameterQueryExpression parameterQueryExpression)
			{
				switch (parameterQueryExpression)
				{
					case ParameterReferenceQueryExpression parameter:
						_writer.WriteParameterName(parameter.ParameterName);
						break;
					case ValueParameterQueryExpression value:
						var parameterName = _writer.Parameters.Add(value.Value);
						_writer.WriteParameterName(parameterName);
						break;
				}
				return base.VisitParameter(parameterQueryExpression);
			}

			public override QueryExpression VisitExtension(QueryExpression extensionQueryExpression)
			{
				_writer.WriteExtension(extensionQueryExpression, this);
				return base.VisitExtension(extensionQueryExpression);
			}
		}
	}
}
