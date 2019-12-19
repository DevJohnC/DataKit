namespace DataKit.SQL.QueryExpressions
{
	public abstract class StatementQueryExpression : ExecutableQueryExpression
	{
		public override ExpressionType ExpressionType => ExpressionType.Statement;

		protected override QueryExpression Accept(QueryExpressionVisitor expressionVisitor)
			=> expressionVisitor.VisitStatement(this);
	}

	public class SelectStatementQueryExpression : StatementQueryExpression
	{
		public SelectStatementQueryExpression(
			ProjectionQueryExpression projection, FromQueryExpression from,
			WhereQueryExpression where, HavingQueryExpression having)
		{
			Projection = projection;
			From = from;
			Where = where;
			Having = having;
		}

		public ProjectionQueryExpression Projection { get; }

		public FromQueryExpression From { get; }

		public WhereQueryExpression Where { get; }

		public HavingQueryExpression Having { get; }
	}
}
