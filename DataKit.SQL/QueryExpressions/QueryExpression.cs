namespace DataKit.SQL.QueryExpressions
{
	public abstract partial class QueryExpression : IQueryExpressionNode
	{
		public abstract ExpressionType ExpressionType { get; }

		protected abstract QueryExpression Accept(QueryExpressionVisitor expressionVisitor);

		QueryExpression IQueryExpressionNode.Accept(QueryExpressionVisitor expressionVisitor)
			=> Accept(expressionVisitor);
	}
}
