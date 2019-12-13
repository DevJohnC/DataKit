using DataKit.Mapping.Binding;
using DataKit.Modelling;
using DataKit.Modelling.TypeModels;
using DataKit.ORM.Schema.Sql;
using DataKit.ORM.Sql.QueryBuilding;
using System;

namespace DataKit.ORM.Sql
{
	public abstract class SqlDataSetBindings
	{
		public SqlDataModel DataModel { get; }

		public SqlDataSetBindings(SqlDataModel dataModel)
		{
			DataModel = dataModel;
		}
	}

	public class SqlDataSetBindings<TEntity> : SqlDataSetBindings
		where TEntity : class
	{
		private readonly CachedCollection<Type, DataModelBinding> _viewToEntityBindingCache
			= new CachedCollection<Type, DataModelBinding>();
		private readonly CachedCollection<Type, DataModelBinding> _entityToViewBindingCache
			= new CachedCollection<Type, DataModelBinding>();
		private readonly CachedCollection<DataModelBinding, ProjectionModel<TEntity>> _projectionModelCache =
			new CachedCollection<DataModelBinding, ProjectionModel<TEntity>>();

		public new SqlDataModel<TEntity> DataModel { get; }

		public SqlDataSetBindings(SqlDataModel<TEntity> dataModel) :
			base(dataModel)
		{
			DataModel = dataModel;
		}

		public ProjectionModel<TEntity, TView> GetProjectionModel<TView>(
			DataModelBinding<TypeModel<TEntity>, PropertyField, TypeModel<TView>, PropertyField> binding = null
			)
			where TView : class
		{
			if (binding == null)
				binding = BindEntityToView<TView>();

			if (_projectionModelCache.TryGetValue(binding, out var projectionModel))
				return projectionModel as ProjectionModel<TEntity, TView>;

			return _projectionModelCache.CreateIfNeeded(binding, () => ProjectionModel<TEntity, TView>.Create(
				DataModel,
				binding
				))
				as ProjectionModel<TEntity, TView>;
		}

		public DataModelBinding<TypeModel<TView>, PropertyField, TypeModel<TEntity>, PropertyField> BindViewToEntity<TView>()
			where TView : class
		{
			if (_viewToEntityBindingCache.TryGetValue(typeof(TView), out var binding))
				return binding as DataModelBinding<TypeModel<TView>, PropertyField, TypeModel<TEntity>, PropertyField>;
			return _viewToEntityBindingCache.CreateIfNeeded(typeof(TView), () => BindViewToEntityImpl<TView>())
				as DataModelBinding<TypeModel<TView>, PropertyField, TypeModel<TEntity>, PropertyField>;
		}

		protected virtual DataModelBinding<TypeModel<TView>, PropertyField, TypeModel<TEntity>, PropertyField> BindViewToEntityImpl<TView>()
			where TView : class
		{
			return BuildDefaultBinding<TView, TEntity>();
		}

		public DataModelBinding<TypeModel<TEntity>, PropertyField, TypeModel<TView>, PropertyField> BindEntityToView<TView>()
			where TView : class
		{
			if (_entityToViewBindingCache.TryGetValue(typeof(TView), out var binding))
				return binding as DataModelBinding<TypeModel<TEntity>, PropertyField, TypeModel<TView>, PropertyField>;
			return _entityToViewBindingCache.CreateIfNeeded(typeof(TView), () => BindEntityToViewImpl<TView>())
				as DataModelBinding<TypeModel<TEntity>, PropertyField, TypeModel<TView>, PropertyField>;
		}

		protected virtual DataModelBinding<TypeModel<TEntity>, PropertyField, TypeModel<TView>, PropertyField> BindEntityToViewImpl<TView>()
			where TView : class
		{
			return BuildDefaultBinding<TEntity, TView>();
		}

		protected virtual DataModelBinding<TypeModel<TSource>, PropertyField, TypeModel<TTarget>, PropertyField> BuildDefaultBinding<TSource, TTarget>()
			where TSource : class
			where TTarget : class
		{
			return new TypeBindingBuilder<TSource, TTarget>()
				.AutoBind()
				.BuildBinding();
		}
	}

	public class SqlDataSetBindings<TBusiness, TEntity> : SqlDataSetBindings<TEntity>
		where TEntity : class
		where TBusiness : class
	{
		private readonly DataModelBinding<TypeModel<TBusiness>, PropertyField, TypeModel<TEntity>, PropertyField> _businessToEntityBinding;
		private readonly DataModelBinding<TypeModel<TEntity>, PropertyField, TypeModel<TBusiness>, PropertyField> _entityToBusinessBinding;

		public SqlDataSetBindings(
			SqlDataModel<TEntity> dataModel,
			DataModelBinding<TypeModel<TBusiness>, PropertyField, TypeModel<TEntity>, PropertyField> businessToEntityBinding = null,
			DataModelBinding<TypeModel<TEntity>, PropertyField, TypeModel<TBusiness>, PropertyField> entityToBusinessBinding = null
			) : base(dataModel)
		{
			_businessToEntityBinding = businessToEntityBinding ?? BuildDefaultBinding<TBusiness, TEntity>();
			_entityToBusinessBinding = entityToBusinessBinding ?? BuildDefaultBinding<TEntity, TBusiness>();
		}

		protected override DataModelBinding<TypeModel<TView>, PropertyField, TypeModel<TEntity>, PropertyField> BindViewToEntityImpl<TView>()
		{
			var viewToBusinessBinding = BuildDefaultBinding<TView, TBusiness>();
			return viewToBusinessBinding.RouteBindingFromSourceType(_businessToEntityBinding);
		}

		protected override DataModelBinding<TypeModel<TEntity>, PropertyField, TypeModel<TView>, PropertyField> BindEntityToViewImpl<TView>()
		{
			var businessToViewBinding = BuildDefaultBinding<TBusiness, TView>();
			return _entityToBusinessBinding.RouteBindingFromSourceType(businessToViewBinding);
		}
	}
}
