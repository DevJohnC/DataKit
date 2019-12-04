using Silk.Data.SQL.Queries;

namespace DataKit.ORM.Sql.Mapping
{
	public class ValueMapper<T> : IResultMapper<T>
	{
		private readonly string _aliasName;

		public ValueMapper(string aliasName)
		{
			_aliasName = aliasName;
		}

		public void InjectSingle(QueryResult queryResult, T instance)
		{
			throw new System.InvalidOperationException("Unable to perform injection on value types.");
		}

		public T MapSingle(QueryResult queryResult)
		{
			var ord = queryResult.GetOrdinal(_aliasName);
			if (queryResult.IsDBNull(ord))
				return default(T);

			if (typeof(T).IsEnum)
				return (T)(object)QueryTypeReaders.GetTypeReader<int>()(queryResult, ord);
			return QueryTypeReaders.GetTypeReader<T>()(queryResult, ord);
		}
	}
}
