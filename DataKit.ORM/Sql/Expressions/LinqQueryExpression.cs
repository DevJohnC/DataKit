using DataKit.ORM.Sql.QueryBuilding;
using DataKit.SQL.QueryExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DataKit.ORM.Sql.Expressions
{
	public class LinqQueryExpression<TEntity> : Expression
		where TEntity : class
	{
		public LinqQueryExpression(QueryExpression queryExpression, IEnumerable<JoinBuilder<TEntity>> joinBuilders = null)
		{
			QueryExpression = queryExpression;
			JoinBuilders = joinBuilders?.ToArray();
		}

		public QueryExpression QueryExpression { get; }

		public JoinBuilder<TEntity>[] JoinBuilders { get; }
	}
}
