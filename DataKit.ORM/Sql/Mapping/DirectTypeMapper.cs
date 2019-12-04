using System;
using DataKit.Mapping;
using DataKit.Modelling.TypeModels;
using DataKit.ORM.Schema.Sql;
using Silk.Data.SQL.Queries;

namespace DataKit.ORM.Sql.Mapping
{
	/// <summary>
	/// Maps fields directly onto TView without creating a TEntity instance.
	/// This has the drawback of not supporting more complex data bindings or logic implemented for properties
	/// on TEntity.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <typeparam name="TView"></typeparam>
	public class DirectTypeMapper<TEntity, TView> : IResultMapper<TView>
		where TEntity : class
		where TView : class
	{
		private readonly SqlStorageModel<TEntity> _model;
		private readonly Mapping<SqlStorageModel<TEntity>, SqlStorageField<TEntity>, TypeModel<TView>, PropertyField> _mapping;
		private readonly IObjectFactory _objectFactory;

		public DirectTypeMapper(
			SqlStorageModel<TEntity> model,
			Mapping<SqlStorageModel<TEntity>, SqlStorageField<TEntity>, TypeModel<TView>, PropertyField> mapping,
			IObjectFactory objectFactory
			)
		{
			_model = model;
			_mapping = mapping;
			_objectFactory = objectFactory;
		}

		public void InjectSingle(QueryResult queryResult, TView instance)
		{
			var reader = new QueryResultModelReader<TEntity>(_model, queryResult);
			var writer = new ObjectDataModelWriter<TView>(instance, _objectFactory);
			_mapping.Run(reader, writer);
		}

		public TView MapSingle(QueryResult queryResult)
		{
			var obj = _objectFactory.CreateInstance<TView>();
			InjectSingle(queryResult, obj);
			return obj;
		}
	}
}
