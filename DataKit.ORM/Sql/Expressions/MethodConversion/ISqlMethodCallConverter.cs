using System.Linq.Expressions;

namespace DataKit.ORM.Sql.Expressions.MethodConversion
{
	public interface ISqlMethodCallConverter
	{
		bool TryConvertMethod<TEntity>(
			MethodCallExpression methodCallExpression,
			ExpressionConversionVisitor<TEntity> expressionConverter,
			out LinqQueryExpression<TEntity> convertedExpression)
			where TEntity : class;
	}
}
