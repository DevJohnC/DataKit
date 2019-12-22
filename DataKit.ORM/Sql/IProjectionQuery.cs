using DataKit.SQL.QueryExpressions;

namespace DataKit.ORM.Sql
{
	public interface IProjectionQuery
	{
		ExecutableQueryExpression BuildQuery();
	}

	public interface IProjectionQuery<TValue> : IProjectionQuery
	{
	}
}
