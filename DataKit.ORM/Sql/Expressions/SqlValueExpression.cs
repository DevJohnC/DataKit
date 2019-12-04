using DataKit.ORM.Sql.QueryBuilding;
using Silk.Data.SQL.Expressions;

namespace DataKit.ORM.Sql.Expressions
{
	public class SqlExpression<TEntity>
		where TEntity : class
	{
		public SqlExpression(QueryExpression queryExpression, JoinBuilder<TEntity>[] joinBuilders)
		{
			QueryExpression = queryExpression;
			Joins = joinBuilders;
		}

		public QueryExpression QueryExpression { get; }

		public JoinBuilder<TEntity>[] Joins { get; }

		public bool RequiresJoins => Joins != null && Joins.Length > 0;
	}

	public class SqlValueExpression<TEntity, TValue> : SqlExpression<TEntity>
		where TEntity : class
	{
		public SqlValueExpression(QueryExpression queryExpression, JoinBuilder<TEntity>[] joinBuilders) :
			base(queryExpression, joinBuilders)
		{
		}
	}
}
