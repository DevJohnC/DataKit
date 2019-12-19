using System;

namespace DataKit.SQL.QueryExpressions
{
	public abstract class OperatorQueryExpression : QueryExpression
	{
		public override ExpressionType ExpressionType => ExpressionType.Operator;

		protected override QueryExpression Accept(QueryExpressionVisitor expressionVisitor)
			=> expressionVisitor.VisitOperator(this);
	}

	public class AsOperatorQueryExpression : OperatorQueryExpression
	{
		public AsOperatorQueryExpression(QueryExpression expression, AliasIdentifierQueryExpression alias)
		{
			Expression = expression ?? throw new ArgumentNullException(nameof(expression));
			Alias = alias ?? throw new ArgumentNullException(nameof(alias));
		}

		public QueryExpression Expression { get; }
		public AliasIdentifierQueryExpression Alias { get; }
	}

	public class IsInOperatorQueryExpression : OperatorQueryExpression
	{
		public IsInOperatorQueryExpression(QueryExpression expression, QueryExpression[] inExpressions)
		{
			Expression = expression ?? throw new ArgumentNullException(nameof(expression));
			InExpressions = inExpressions ?? throw new ArgumentNullException(nameof(inExpressions));
			if (inExpressions.Length == 0)
				throw new ArgumentException("Must provide at least 1 expression.", nameof(inExpressions));
		}

		public QueryExpression Expression { get; }
		public QueryExpression[] InExpressions { get; }
	}
}
