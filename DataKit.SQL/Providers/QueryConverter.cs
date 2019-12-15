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
					case WhereQueryExpression where:
						_writer.WriteWhereQueryParameters(where, this);
						break;
					case HavingQueryExpression having:
						_writer.WriteHavingQueryParameters(having, this);
						break;
				}
				return base.VisitQueryParameter(queryParameterQueryExpression);
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
		}
	}
}
