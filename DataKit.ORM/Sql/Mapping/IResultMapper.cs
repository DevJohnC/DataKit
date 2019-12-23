using DataKit.SQL.Providers;

namespace DataKit.ORM.Sql.Mapping
{
	public interface IResultMapper<T>
	{
		T MapSingle(QueryResult queryResult);

		void InjectSingle(QueryResult queryResult, T instance);
	}
}
