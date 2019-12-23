using System;

namespace DataKit.SQL.QueryExpressions
{
	public abstract class FunctionCallQueryExpression : QueryExpression
	{
		public override ExpressionType ExpressionType => ExpressionType.FunctionCall;

		protected override QueryExpression Accept(QueryExpressionVisitor expressionVisitor)
			=> expressionVisitor.VisitFunctionCall(this);
	}

	public class CountFunctionCallQueryExpression : FunctionCallQueryExpression
	{
		public CountFunctionCallQueryExpression(QueryExpression expression)
		{
			Expression = expression;
		}

		public QueryExpression Expression { get; }
	}

	public class RandomFunctionCallQueryExpression : FunctionCallQueryExpression
	{
	}

	public class MinFunctionCallQueryExpression : FunctionCallQueryExpression
	{
		public MinFunctionCallQueryExpression(QueryExpression expression)
		{
			Expression = expression;
		}

		public QueryExpression Expression { get; }
	}

	public class MaxFunctionCallQueryExpression : FunctionCallQueryExpression
	{
		public MaxFunctionCallQueryExpression(QueryExpression expression)
		{
			Expression = expression;
		}

		public QueryExpression Expression { get; }
	}

	public class ConcatFunctionCallQueryExpression : FunctionCallQueryExpression
	{
		public ConcatFunctionCallQueryExpression(QueryExpression[] expressions)
		{
			Expressions = expressions ?? throw new ArgumentNullException(nameof(expressions));
		}

		public QueryExpression[] Expressions { get; }
	}

	public class LastInsertedIdFunctionCallExpression : FunctionCallQueryExpression
	{
	}
}
