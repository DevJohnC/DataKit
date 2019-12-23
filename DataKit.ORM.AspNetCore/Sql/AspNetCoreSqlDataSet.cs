using DataKit.Mapping;
using DataKit.Mapping.AspNetCore.Mapping;
using DataKit.Mapping.Binding;
using DataKit.Modelling.TypeModels;
using DataKit.ORM.Schema.Sql;
using IQueryProvider = DataKit.SQL.Providers.IQueryProvider;

namespace DataKit.ORM.AspNetCore.Sql
{
	public class AspNetCoreSqlDataSet<TEntity> : SqlDataSet<TEntity>
		where TEntity : class
	{
		public AspNetCoreSqlDataSet(SqlDataModel<TEntity> dataModel, IQueryProvider queryProvider,
			IObjectFactory objectFactory, BindingProvider bindingProvider) :
			base(dataModel, queryProvider, objectFactory)
		{
			Bindings = new AspNetCoreSqlDataSetBindings<TEntity>(dataModel, bindingProvider);
		}
	}

	public class AspNetCoreSqlDataSet<TBusiness, TEntity> : SqlDataSet<TBusiness, TEntity>
		where TEntity : class
		where TBusiness : class
	{
		public AspNetCoreSqlDataSet(SqlDataModel<TEntity> dataModel, IQueryProvider queryProvider,
			IObjectFactory objectFactory, BindingProvider bindingProvider,
			DataModelBinding<TypeModel<TBusiness>, PropertyField, TypeModel<TEntity>, PropertyField> businessToEntityBinding = null,
			DataModelBinding<TypeModel<TEntity>, PropertyField, TypeModel<TBusiness>, PropertyField> entityToBusinessBinding = null) :
			base(dataModel, queryProvider, objectFactory, businessToEntityBinding, entityToBusinessBinding)
		{
			Bindings = new AspNetCoreSqlDataSetBindings<TBusiness, TEntity>(dataModel, bindingProvider);
		}
	}
}
