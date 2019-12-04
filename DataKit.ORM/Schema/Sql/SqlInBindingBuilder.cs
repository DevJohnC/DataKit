using DataKit.Mapping.Binding;
using DataKit.Modelling.TypeModels;
using System.Collections.Generic;

namespace DataKit.ORM.Schema.Sql
{
	public class SqlInBindingBuilder<TEntity> :
		DataModelBindingBuilder<SqlInBindingBuilder<TEntity>, DataModelInBinding<TEntity>, TypeModel, PropertyField, SqlStorageModel<TEntity>, SqlStorageField<TEntity>>
		where TEntity : class
	{
		public SqlInBindingBuilder(SqlEntityModel<TEntity> sourceModel, SqlStorageModel<TEntity> targetModel) :
			base(sourceModel, targetModel)
		{
		}

		protected override DataModelInBinding<TEntity> BindingFactory(TypeModel sourceModel, SqlStorageModel<TEntity> targetModel, IReadOnlyList<ModelFieldBinding<PropertyField, SqlStorageField<TEntity>>> fieldBindings)
		{
			return new DataModelInBinding<TEntity>(
				sourceModel as TypeModel<TEntity>,
				targetModel,
				fieldBindings
				);
		}
	}
}
