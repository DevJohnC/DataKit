using DataKit.SQL.Providers;
using DataKit.SQL.QueryExpressions;
using System.Threading.Tasks;

namespace DataKit.ORM.Sql
{
	/// <summary>
	/// Base class for sql data operations.
	/// </summary>
	public abstract class SqlDataOperationBase<TEntity> : DataOperation<SqlDataSet<TEntity>>
		where TEntity : class
	{
		protected SqlDataOperationBase(SqlDataSet<TEntity> dataSet, IQueryProvider queryProvider) : base(dataSet)
		{
			QueryProvider = queryProvider;
		}

		public IQueryProvider QueryProvider { get; }

		public SqlServerFunctions ServerFunctions { get; }

		protected abstract ExecutableQueryExpression BuildQuery();
	}

	/// <summary>
	/// Non-mapping sql data operation.
	/// </summary>
	/// <remarks>
	/// Usually all operations that aren't SELECTing are this type.
	/// </remarks>
	/// <typeparam name="TEntity"></typeparam>
	public abstract class SqlDataOperation<TEntity> : SqlDataOperationBase<TEntity>, ISqlBatchableOperation
		where TEntity : class
	{
		protected SqlDataOperation(SqlDataSet<TEntity> dataSet, IQueryProvider queryProvider) : base(dataSet, queryProvider)
		{
		}

		public virtual int Execute()
		{
			return QueryProvider.ExecuteNonQuery(BuildQuery());
		}

		public virtual Task<int> ExecuteAsync()
		{
			return QueryProvider.ExecuteNonQueryAsync(BuildQuery());
		}

		ExecutableQueryExpression ISqlBatchableOperation.BuildQuery()
			=> BuildQuery();
	}
}
