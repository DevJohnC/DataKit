namespace DataKit.SQL.QueryExpressions
{
	public class QueryExpressionVisitor
	{
		public virtual QueryExpression Visit(QueryExpression queryExpression)
		{
			return ((IQueryExpressionNode)queryExpression).Accept(this);
		}

		public virtual QueryExpression VisitStatement(StatementQueryExpression statementQueryExpression)
		{
			return statementQueryExpression;
		}

		public virtual QueryExpression VisitProjection(ProjectionQueryExpression projectionQueryExpression)
		{
			return projectionQueryExpression;
		}

		public virtual QueryExpression VisitQueryParameter(QueryParameterQueryExpression queryParameterQueryExpression)
		{
			return queryParameterQueryExpression;
		}

		public virtual QueryExpression VisitFunctionCall(FunctionCallQueryExpression functionCallQueryExpression)
		{
			return functionCallQueryExpression;
		}
	}
}
