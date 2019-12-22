using DataKit.Mapping;
using DataKit.Modelling.TypeModels;
using DataKit.ORM.Schema.Sql;
using DataKit.SQL.Providers;

namespace DataKit.ORM.Sql.Mapping
{
	/// <summary>
	/// Maps fields onto a TView instance by first building a TEntity instance.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <typeparam name="TView"></typeparam>
	public class EntityTypeMapper<TEntity, TView> : IResultMapper<TView>
		where TEntity : class
		where TView : class
	{
		private readonly SqlStorageModel<TEntity> _storageModel;
		private readonly Mapping<SqlStorageModel<TEntity>, SqlStorageField<TEntity>, TypeModel<TEntity>, PropertyField> _toEntityMapping;
		private readonly Mapping<TypeModel<TEntity>, PropertyField, TypeModel<TView>, PropertyField> _toViewMapping;
		private readonly IObjectFactory _objectFactory;

		public EntityTypeMapper(
			SqlStorageModel<TEntity> storageModel,
			Mapping<SqlStorageModel<TEntity>, SqlStorageField<TEntity>, TypeModel<TEntity>, PropertyField> toEntityMapping,
			Mapping<TypeModel<TEntity>, PropertyField, TypeModel<TView>, PropertyField> toViewMapping,
			IObjectFactory objectFactory
			)
		{
			_storageModel = storageModel;
			_toEntityMapping = toEntityMapping;
			_toViewMapping = toViewMapping;
			_objectFactory = objectFactory;
		}

		public void InjectSingle(QueryResult queryResult, TView instance)
		{
			var entity = _objectFactory.CreateInstance<TEntity>();
			var entityReader = new QueryResultModelReader<TEntity>(_storageModel, queryResult);
			var entityWriter = new ObjectDataModelWriter<TEntity>(entity, _objectFactory);
			_toEntityMapping.Run(entityReader, entityWriter);

			var viewReader = new ObjectDataModelReader<TEntity>(entity);
			var viewWriter = new ObjectDataModelWriter<TView>(instance, _objectFactory);
			_toViewMapping.Run(viewReader, viewWriter);
		}

		public TView MapSingle(QueryResult queryResult)
		{
			var view = _objectFactory.CreateInstance<TView>();
			InjectSingle(queryResult, view);
			return view;
		}
	}
}
