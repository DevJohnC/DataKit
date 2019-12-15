using System;

namespace DataKit.SQL.QueryExpressions
{
	public abstract class QueryParameterQueryExpression : QueryExpression
	{
		public override ExpressionType ExpressionType => ExpressionType.QueryParameter;

		protected override QueryExpression Accept(QueryExpressionVisitor expressionVisitor)
			=> expressionVisitor.VisitQueryParameter(this);
	}

	public class FromQueryExpression : QueryParameterQueryExpression
	{
		public FromQueryExpression(QueryExpression from)
		{
			From = from ?? throw new ArgumentNullException(nameof(from));
		}

		public QueryExpression From { get; }
	}

	public class WhereQueryExpression : QueryParameterQueryExpression
	{
		public WhereQueryExpression(QueryExpression where)
		{
			Where = where ?? throw new ArgumentNullException(nameof(where));
		}

		public QueryExpression Where { get; }
	}

	public class HavingQueryExpression : QueryParameterQueryExpression
	{
		public HavingQueryExpression(QueryExpression having)
		{
			Having = having ?? throw new ArgumentNullException(nameof(having));
		}

		public QueryExpression Having { get; }
	}
}
