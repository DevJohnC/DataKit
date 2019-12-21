using System;
using System.Collections.Generic;

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
			Expression = from ?? throw new ArgumentNullException(nameof(from));
		}

		public QueryExpression Expression { get; }
	}

	public class WhereQueryExpression : QueryParameterQueryExpression
	{
		public WhereQueryExpression(QueryExpression where)
		{
			Expression = where ?? throw new ArgumentNullException(nameof(where));
		}

		public QueryExpression Expression { get; }
	}

	public class HavingQueryExpression : QueryParameterQueryExpression
	{
		public HavingQueryExpression(QueryExpression having)
		{
			Expression = having ?? throw new ArgumentNullException(nameof(having));
		}

		public QueryExpression Expression { get; }
	}

	public class OrderByQueryExpression : QueryParameterQueryExpression
	{
		public OrderByQueryExpression(QueryExpression expression, OrderByDirection direction)
		{
			Expression = expression ?? throw new ArgumentNullException(nameof(expression));
			Direction = direction;
		}

		public QueryExpression Expression { get; }

		public OrderByDirection Direction { get; }
	}

	public class OrderByCollectionQueryExpression : QueryParameterQueryExpression
	{
		public OrderByCollectionQueryExpression(IReadOnlyList<OrderByQueryExpression> expressions)
		{
			Expressions = expressions ?? throw new ArgumentNullException(nameof(expressions));
		}

		public IReadOnlyList<OrderByQueryExpression> Expressions { get; }
	}

	public class GroupByQueryExpression : QueryParameterQueryExpression
	{
		public GroupByQueryExpression(QueryExpression expression)
		{
			Expression = expression ?? throw new ArgumentNullException(nameof(expression));
		}

		public QueryExpression Expression { get; }
	}

	public class GroupByCollectionQueryExpression : QueryParameterQueryExpression
	{
		public GroupByCollectionQueryExpression(IReadOnlyList<GroupByQueryExpression> expressions)
		{
			Expressions = expressions ?? throw new ArgumentNullException(nameof(expressions));
		}

		public IReadOnlyList<GroupByQueryExpression> Expressions { get; }
	}

	public class LimitQueryExpression : QueryParameterQueryExpression
	{
		public LimitQueryExpression(QueryExpression limit, QueryExpression offset = null)
		{
			Limit = limit ?? throw new ArgumentNullException(nameof(limit));
			Offset = offset;
		}

		public QueryExpression Offset { get; }
		public new QueryExpression Limit { get; }
	}

	public enum OrderByDirection
	{
		Ascending,
		Descending
	}
}
