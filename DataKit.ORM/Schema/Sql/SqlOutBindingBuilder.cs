using DataKit.Mapping.Binding;
using DataKit.Modelling.TypeModels;
using System.Collections.Generic;

namespace DataKit.ORM.Schema.Sql
{
	public class SqlOutBindingBuilder<TEntity> :
		DataModelBindingBuilder<SqlOutBindingBuilder<TEntity>, DataModelOutBinding<TEntity>, SqlStorageModel<TEntity>, SqlStorageField<TEntity>, TypeModel, PropertyField>
		where TEntity : class
	{
		public SqlOutBindingBuilder(SqlStorageModel<TEntity> sourceModel, SqlEntityModel<TEntity> targetModel) :
			base(sourceModel, targetModel)
		{
		}

		protected override DataModelOutBinding<TEntity> BindingFactory(SqlStorageModel<TEntity> sourceModel, TypeModel targetModel, IReadOnlyList<ModelFieldBinding<SqlStorageField<TEntity>, PropertyField>> fieldBindings)
		{
			return new DataModelOutBinding<TEntity>(
				sourceModel,
				targetModel as TypeModel<TEntity>,
				fieldBindings
				);
		}
	}
}
