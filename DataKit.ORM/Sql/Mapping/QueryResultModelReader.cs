using DataKit.Mapping;
using DataKit.ORM.Schema.Sql;
using Silk.Data.SQL.Queries;

namespace DataKit.ORM.Sql.Mapping
{
	public class QueryResultModelReader<TEntity> : IDataModelReader<SqlStorageModel<TEntity>, SqlStorageField<TEntity>>
		where TEntity : class
	{
		public SqlStorageModel<TEntity> Model { get; }

		private readonly QueryResult _queryResult;

		public QueryResultModelReader(SqlStorageModel<TEntity> model, QueryResult queryResult)
		{
			Model = model;
			_queryResult = queryResult;
		}

		public void EnterEnumerable()
		{
			throw new System.NotImplementedException();
		}

		public void EnterMember(SqlStorageField<TEntity> field)
		{
			//  noop
		}

		public void LeaveEnumerable()
		{
			throw new System.NotImplementedException();
		}

		public void LeaveMember()
		{
			//  noop
		}

		public bool MoveNext()
		{
			throw new System.NotImplementedException();
		}

		public T ReadField<T>(SqlStorageField<TEntity> field)
		{
			var ord = _queryResult.GetOrdinal(field.FieldName);
			if (_queryResult.IsDBNull(ord))
				return default(T);

			if (typeof(T).IsEnum)
				return (T)(object)QueryTypeReaders.GetTypeReader<int>()(_queryResult, ord);
			var reader = QueryTypeReaders.GetTypeReader<T>();
			if (reader == null)
				throw new System.InvalidOperationException("Attempting to read an unsupported data type from query result.");
			return reader(_queryResult, ord);
		}
	}
}
