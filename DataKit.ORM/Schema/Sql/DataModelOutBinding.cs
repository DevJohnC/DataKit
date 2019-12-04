using System.Collections.Generic;
using DataKit.Mapping;
using DataKit.Mapping.Binding;
using DataKit.Modelling.TypeModels;

namespace DataKit.ORM.Schema.Sql
{
	public class SqlStorageOutBinding<TEntity, TView> : DataModelBinding<SqlStorageModel<TEntity>, SqlStorageField<TEntity>, TypeModel<TView>, PropertyField>
		where TEntity : class
		where TView : class
	{
		public SqlStorageOutBinding(SqlStorageModel<TEntity> sourceModel, TypeModel<TView> targetModel, IReadOnlyList<ModelFieldBinding<SqlStorageField<TEntity>, PropertyField>> fieldBindings) :
			base(sourceModel, targetModel, fieldBindings)
		{
		}

		private Mapping<SqlStorageModel<TEntity>, SqlStorageField<TEntity>, TypeModel<TView>, PropertyField> _mapping;
		public Mapping<SqlStorageModel<TEntity>, SqlStorageField<TEntity>, TypeModel<TView>, PropertyField> Mapping
		{
			get
			{
				if (_mapping == null)
					_mapping = BuildMapping();
				return _mapping;
			}
		}

		private readonly object _syncLock = new object();
		private Mapping<SqlStorageModel<TEntity>, SqlStorageField<TEntity>, TypeModel<TView>, PropertyField> BuildMapping()
		{
			lock (_syncLock)
			{
				if (_mapping != null)
					return _mapping;

				var builder = new MappingBuilder<SqlStorageModel<TEntity>, SqlStorageField<TEntity>, TypeModel<TView>, PropertyField>(this);
				return builder.Build();
			}
		}
	}

	public class DataModelOutBinding<TEntity> : SqlStorageOutBinding<TEntity, TEntity>
		where TEntity : class
	{
		public DataModelOutBinding(SqlStorageModel<TEntity> sourceModel, TypeModel<TEntity> targetModel, IReadOnlyList<ModelFieldBinding<SqlStorageField<TEntity>, PropertyField>> fieldBindings) :
			base(sourceModel, targetModel, fieldBindings)
		{
		}
	}
}
