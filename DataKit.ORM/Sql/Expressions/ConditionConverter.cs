using DataKit.ORM.Schema.Sql;
using System;
using System.Linq.Expressions;

namespace DataKit.ORM.Sql.Expressions
{
	public class ConditionConverter<TEntity>
		where TEntity : class
	{
		private ExpressionConversionVisitor<TEntity> _visitor;

		public IAliasIdentifier TableIdentifier => _visitor.TableIdentifier;

		public ConditionConverter(SqlDataModel<TEntity> dataModel, IAliasIdentifier tableIdentifier)
		{
			_visitor = new ExpressionConversionVisitor<TEntity>(dataModel, tableIdentifier);
		}

		public SqlValueExpression<TEntity, bool> ConvertClause(Expression<Func<TEntity, bool>> clauseExpression)
		{
			var convertedExpression = _visitor.VisitMain(clauseExpression);
			if (convertedExpression is LinqQueryExpression<TEntity> linqQueryExpression)
				return new SqlValueExpression<TEntity, bool>(linqQueryExpression.QueryExpression, linqQueryExpression.JoinBuilders);
			throw new InvalidOperationException("Expression couldn't be translated to an SQL query expression.");
		}
	}
}
