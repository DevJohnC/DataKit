using DataKit.ORM.Schema.Sql;
using System;
using System.Linq.Expressions;

namespace DataKit.ORM.Sql.Expressions
{
	public class ValueConverter<TEntity>
		where TEntity : class
	{
		private readonly ExpressionConversionVisitor<TEntity> _visitor;

		public IAliasIdentifier TableIdentifier => _visitor.TableIdentifier;

		public ValueConverter(SqlDataModel<TEntity> dataModel, IAliasIdentifier tableIdentifier)
		{
			_visitor = new ExpressionConversionVisitor<TEntity>(dataModel, tableIdentifier);
		}

		public SqlValueExpression<TEntity, TValue> ConvertExpression<TValue>(
			Expression<Func<TEntity, TValue>> valueExpression
			)
		{
			var convertedExpression = _visitor.VisitMain(valueExpression);
			if (convertedExpression is LinqQueryExpression<TEntity> linqQueryExpression)
				return new SqlValueExpression<TEntity, TValue>(linqQueryExpression.QueryExpression, linqQueryExpression.JoinBuilders);
			throw new InvalidOperationException("Expression couldn't be translated to an SQL query expression.");
		}
	}
}
