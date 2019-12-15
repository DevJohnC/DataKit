using System;

namespace DataKit.SQL.QueryExpressions
{
	public class ProjectionQueryExpression : QueryExpression
	{
		public ProjectionQueryExpression(QueryExpression[] expressions)
		{
			Expressions = expressions ?? throw new ArgumentNullException(nameof(expressions));
		}

		public override ExpressionType ExpressionType => ExpressionType.Projection;

		public QueryExpression[] Expressions { get; }

		protected override QueryExpression Accept(QueryExpressionVisitor expressionVisitor)
			=> expressionVisitor.VisitProjection(this);
	}
}
