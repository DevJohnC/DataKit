using DataKit.Mapping.Binding;
using DataKit.Modelling;
using DataKit.Modelling.TypeModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataKit.ORM.Schema.Sql
{
	public abstract class SqlDataModel : IDataSchemaAssignment
	{
		public SqlStorageModel StorageModel { get; }

		public DataSchema DataSchema { get; private set; }

		public abstract Type EntityType { get; }

		protected SqlDataModel(SqlStorageModel storageModel)
		{
			StorageModel = storageModel;
		}

		void IDataSchemaAssignment.SetDataSchemaAndSeal(DataSchema dataSchema)
		{
			if (DataSchema != null)
				throw new System.InvalidOperationException("DataSchema is sealed.");
			DataSchema = dataSchema;
		}
	}

	public class SqlDataModel<TEntity> : SqlDataModel
		where TEntity : class
	{
		private readonly CachedCollection<DataModelBinding, DataModelBinding> _storageInBindingCache
			= new CachedCollection<DataModelBinding, DataModelBinding>();

		private readonly CachedCollection<DataModelBinding, DataModelBinding> _storageOutBindingCache
			= new CachedCollection<DataModelBinding, DataModelBinding>();

		public SqlEntityModel<TEntity> EntityModel { get; }

		public new SqlStorageModel<TEntity> StorageModel { get; }

		public DataModelInBinding<TEntity> InBinding { get; }

		public DataModelOutBinding<TEntity> OutBinding { get; }

		public override Type EntityType => typeof(TEntity);

		public SqlDataModel(
			SqlEntityModel<TEntity> entityModel, SqlStorageModel<TEntity> storageModel,
			DataModelInBinding<TEntity> inBinding, DataModelOutBinding<TEntity> outBinding) :
			base(storageModel)
		{
			EntityModel = entityModel;
			StorageModel = storageModel;
			InBinding = inBinding;
			OutBinding = outBinding;
		}

		public DataModelBinding<TypeModel<TView>, PropertyField, SqlStorageModel<TEntity>, SqlStorageField<TEntity>> GetInStorageBinding<TView>(
			DataModelBinding<TypeModel<TView>, PropertyField, TypeModel<TEntity>, PropertyField> entityBinding,
			bool ignoreCaching = false
			)
		{
			if (ignoreCaching)
				return entityBinding.RouteBindingToTargetType(InBinding);

			if (_storageInBindingCache.TryGetValue(entityBinding, out var storageBinding))
				return storageBinding as DataModelBinding<TypeModel<TView>, PropertyField, SqlStorageModel<TEntity>, SqlStorageField<TEntity>>;

			return _storageInBindingCache.CreateIfNeeded(entityBinding, () => entityBinding.RouteBindingToTargetType(InBinding))
				 as DataModelBinding<TypeModel<TView>, PropertyField, SqlStorageModel<TEntity>, SqlStorageField<TEntity>>;
		}

		public SqlStorageOutBinding<TEntity, TView> GetOutStorageBinding<TView>(
			DataModelBinding<TypeModel<TEntity>, PropertyField, TypeModel<TView>, PropertyField> entityBinding,
			bool ignoreCaching = false
			)
			where TView : class
		{
			if (ignoreCaching)
				return Repack(OutBinding.RouteBindingFromSourceType(entityBinding));

			if (_storageOutBindingCache.TryGetValue(entityBinding, out var storageBinding))
				return storageBinding as SqlStorageOutBinding<TEntity, TView>;

			return _storageOutBindingCache.CreateIfNeeded(entityBinding, () => Repack(OutBinding.RouteBindingFromSourceType(entityBinding)))
				 as SqlStorageOutBinding<TEntity, TView>;
		}

		public IEnumerable<ModelFieldBinding<SqlStorageField<TEntity>, PropertyField>> GetRequiredStorageFields<TView>(
			DataModelBinding<TypeModel<TEntity>, PropertyField, TypeModel<TView>, PropertyField> entityBinding
			)
			where TView : class
		{
			foreach (var fieldBinding in entityBinding.FieldBindings)
			{
				if (!fieldBinding.TargetField.CanWrite)
					continue;

				foreach (var storageBinding in OutBinding.FieldBindings.Where(
					q => q.BindingTarget.Path.Take(fieldBinding.BindingSource.Path.Length).SequenceEqual(fieldBinding.BindingSource.Path)
					))
				{
					yield return storageBinding;
				}
			}
		}

		private SqlStorageOutBinding<TEntity, TView> Repack<TView>(
			DataModelBinding<SqlStorageModel<TEntity>, SqlStorageField<TEntity>, TypeModel<TView>, PropertyField> binding
			)
			where TView : class
		{
			return new SqlStorageOutBinding<TEntity, TView>(
				binding.SourceModel,
				binding.TargetModel,
				binding.FieldBindings
				);
		}
	}
}
