using DataKit.Mapping.AspNetCore.Mapping;
using DataKit.Mapping.Binding;
using DataKit.Modelling.TypeModels;
using DataKit.ORM.Schema.Sql;
using DataKit.ORM.Sql;

namespace DataKit.ORM.AspNetCore.Sql
{
	public class AspNetCoreSqlDataSetBindings<TEntity> : SqlDataSetBindings<TEntity>
		where TEntity : class
	{
		private readonly BindingProvider _bindingProvider;

		public AspNetCoreSqlDataSetBindings(SqlDataModel<TEntity> dataModel, BindingProvider bindingProvider) : base(dataModel)
		{
			_bindingProvider = bindingProvider;
		}

		protected override DataModelBinding<TypeModel<TSource>, PropertyField, TypeModel<TTarget>, PropertyField> BuildDefaultBinding<TSource, TTarget>()
		{
			return _bindingProvider.GetBinding<TSource, TTarget>();
		}
	}

	public class AspNetCoreSqlDataSetBindings<TBusiness, TEntity> : SqlDataSetBindings<TBusiness, TEntity>
		where TEntity : class
		where TBusiness : class
	{
		private readonly BindingProvider _bindingProvider;

		public AspNetCoreSqlDataSetBindings(SqlDataModel<TEntity> dataModel, BindingProvider bindingProvider) : base(dataModel)
		{
			_bindingProvider = bindingProvider;
		}

		protected override DataModelBinding<TypeModel<TSource>, PropertyField, TypeModel<TTarget>, PropertyField> BuildDefaultBinding<TSource, TTarget>()
		{
			return _bindingProvider.GetBinding<TSource, TTarget>();
		}
	}
}
