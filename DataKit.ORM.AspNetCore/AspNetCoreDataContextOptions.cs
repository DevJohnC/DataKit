using DataKit.Mapping;
using DataKit.Mapping.AspNetCore.Mapping;
using DataKit.ORM.AspNetCore.Sql;
using DataKit.ORM.Schema;
using DataKit.ORM.Schema.Sql;
using System.Linq;
using IQueryProvider = DataKit.SQL.Providers.IQueryProvider;

namespace DataKit.ORM.AspNetCore
{
	public class AspNetCoreDataContextOptions : DataContextCreationOptions
	{
		private readonly IObjectFactory _objectFactory;
		private readonly BindingProvider _bindingProvider;

		public AspNetCoreDataContextOptions(IObjectFactory objectFactory, BindingProvider bindingProvider)
		{
			_objectFactory = objectFactory;
			_bindingProvider = bindingProvider;
		}

		public override SqlDataSet<TBusiness, TEntity> SqlDataSetFactory<TBusiness, TEntity>(DataSchema dataSchema, IQueryProvider queryProvider)
		{
			return new AspNetCoreSqlDataSet<TBusiness, TEntity>(
				dataSchema.Sql.SqlEntities.OfType<SqlDataModel<TEntity>>().First(),
				queryProvider,
				_objectFactory,
				_bindingProvider
				);
		}

		public override SqlDataSet<TEntity> SqlDataSetFactory<TEntity>(DataSchema dataSchema, IQueryProvider queryProvider)
		{
			return new AspNetCoreSqlDataSet<TEntity>(
				dataSchema.Sql.SqlEntities.OfType<SqlDataModel<TEntity>>().First(),
				queryProvider,
				_objectFactory,
				_bindingProvider
				);
		}
	}
}
