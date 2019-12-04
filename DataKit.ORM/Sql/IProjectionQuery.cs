using Silk.Data.SQL.Expressions;

namespace DataKit.ORM.Sql
{
	public interface IProjectionQuery
	{
		QueryExpression BuildQuery();
	}

	public interface IProjectionQuery<TValue> : IProjectionQuery
	{
	}
}
