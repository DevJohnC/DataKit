namespace DataKit.SQL.QueryExpressions
{
	public abstract class FunctionCallQueryExpression : QueryExpression
	{
		public override ExpressionType ExpressionType => ExpressionType.FunctionCall;

		protected override QueryExpression Accept(QueryExpressionVisitor expressionVisitor)
			=> expressionVisitor.VisitFunctionCall(this);
	}

	public class CountFunctionCallQueryExpression : FunctionCallQueryExpression
	{
		public CountFunctionCallQueryExpression(QueryExpression expression)
		{
			Expression = expression;
		}

		public QueryExpression Expression { get; }
	}
}
