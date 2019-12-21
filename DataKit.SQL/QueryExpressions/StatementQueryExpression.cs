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
			WhereQueryExpression where, HavingQueryExpression having,
			OrderByQueryExpression[] orderBy, GroupByQueryExpression[] groupBy,
			LimitQueryExpression limit)
		{
			Projection = projection;
			From = from;
			Where = where;
			Having = having;
			OrderBy = (orderBy == null || orderBy.Length < 1) ? null : new OrderByCollectionQueryExpression(orderBy);
			GroupBy = (groupBy == null || groupBy.Length < 1) ? null : new GroupByCollectionQueryExpression(groupBy);
			Limit = limit;
		}

		public ProjectionQueryExpression Projection { get; }

		public FromQueryExpression From { get; }

		public WhereQueryExpression Where { get; }

		public HavingQueryExpression Having { get; }

		public new OrderByCollectionQueryExpression OrderBy { get; }

		public new GroupByCollectionQueryExpression GroupBy { get; }

		public new LimitQueryExpression Limit { get; }
	}
}
