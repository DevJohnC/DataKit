namespace DataKit.SQL.QueryExpressions
{
	public interface IQueryExpressionNode
	{
		QueryExpression Accept(QueryExpressionVisitor expressionVisitor);
	}
}
