namespace DataKit.SQL.QueryExpressions
{
	public abstract class ExecutableQueryExpression : QueryExpression
	{
	}

	public abstract class ExecutableExtensionQueryExpression : ExecutableQueryExpression
	{
		public override ExpressionType ExpressionType => ExpressionType.Extension;

		protected override QueryExpression Accept(QueryExpressionVisitor expressionVisitor)
			=> expressionVisitor.VisitExtension(this);
	}
}
