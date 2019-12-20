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
		public IsInOperatorQueryExpression(QueryExpression expression, QueryExpression[] inExpressions,
			bool isNotIn)
		{
			Expression = expression ?? throw new ArgumentNullException(nameof(expression));
			InExpressions = inExpressions ?? throw new ArgumentNullException(nameof(inExpressions));
			if (inExpressions.Length == 0)
				throw new ArgumentException("Must provide at least 1 expression.", nameof(inExpressions));
			IsNotIn = isNotIn;
		}

		public QueryExpression Expression { get; }
		public QueryExpression[] InExpressions { get; }
		public new bool IsNotIn { get; }
	}

	public class UnaryOperatorQueryExpression : OperatorQueryExpression
	{
		public UnaryOperatorQueryExpression(QueryExpression expression, UnaryOperator @operator)
		{
			Expression = expression ?? throw new ArgumentNullException(nameof(expression));
			Operator = @operator;
		}

		public QueryExpression Expression { get; }

		public UnaryOperator Operator { get; }
	}

	public class BinaryOperatorQueryExpression : OperatorQueryExpression
	{
		public BinaryOperatorQueryExpression(QueryExpression left, QueryExpression right, BinaryOperator @operator)
		{
			Left = left ?? throw new ArgumentNullException(nameof(left));
			Right = right ?? throw new ArgumentNullException(nameof(right));
			Operator = @operator;
		}

		public QueryExpression Left { get; }
		public QueryExpression Right { get; }
		public BinaryOperator Operator { get; }
	}

	public enum UnaryOperator
	{
		IsNull,
		IsNotNull
	}

	public enum BinaryOperator
	{
		AreEqual,
		AreNotEqual,
		GreaterThan,
		LessThan,
		GreaterThanOrEqualTo,
		LessThanOrEqualTo,
		Like,
		NotLike,

		AndAlso,
		OrElse,

		Addition,
		Subtraction,
		Multiplication,
		Division,
		Modulus,

		BitwiseAnd,
		BitwiseOr,
		BitwiseExclusiveOr
	}
}
