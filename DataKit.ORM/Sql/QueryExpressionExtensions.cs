using DataKit.SQL.QueryExpressions;

namespace DataKit.ORM.Sql
{
	internal static class QueryExpressionExtensions
	{
		public static QueryExpression OrElse(this QueryExpression left, QueryExpression right)
		{
			if (left == null)
				return right;
			return QueryExpression.OrElse(left, right);
		}

		public static QueryExpression AndAlso(this QueryExpression left, QueryExpression right)
		{
			if (left == null)
				return right;
			return QueryExpression.AndAlso(left, right);
		}
	}
}
