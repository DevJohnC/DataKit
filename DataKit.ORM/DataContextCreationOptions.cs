using DataKit.ORM.Schema;
using DataKit.ORM.Schema.Sql;
using System.Linq;
using IQueryProvider = DataKit.SQL.Providers.IQueryProvider;

namespace DataKit.ORM
{
	public class DataContextCreationOptions
	{
		public static DataContextCreationOptions Default { get; }
			= new DataContextCreationOptions();

		public virtual SqlDataSet<TEntity> SqlDataSetFactory<TEntity>(DataSchema dataSchema, IQueryProvider queryProvider)
			where TEntity : class
		{
			return new SqlDataSet<TEntity>(
				dataSchema.Sql.SqlEntities.OfType<SqlDataModel<TEntity>>().First(),
				queryProvider
				);
		}

		public virtual SqlDataSet<TBusiness, TEntity> SqlDataSetFactory<TBusiness, TEntity>(DataSchema dataSchema, IQueryProvider queryProvider)
			where TBusiness : class
			where TEntity : class
		{
			return new SqlDataSet<TBusiness, TEntity>(
				dataSchema.Sql.SqlEntities.OfType<SqlDataModel<TEntity>>().First(),
				queryProvider
				);
		}
	}
}
