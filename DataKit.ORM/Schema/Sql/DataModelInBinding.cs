using System.Collections.Generic;
using System.Linq;
using DataKit.Mapping.Binding;
using DataKit.Modelling.TypeModels;

namespace DataKit.ORM.Schema.Sql
{
	public class DataModelInBinding<TEntity> : DataModelBinding<TypeModel<TEntity>, PropertyField, SqlStorageModel<TEntity>, SqlStorageField<TEntity>>
		where TEntity : class
	{
		public DataModelInBinding(TypeModel<TEntity> sourceModel, SqlStorageModel<TEntity> targetModel, IReadOnlyList<ModelFieldBinding<PropertyField, SqlStorageField<TEntity>>> fieldBindings) :
			base(sourceModel, targetModel, fieldBindings)
		{
		}

		public ModelFieldBinding<PropertyField, SqlStorageField<TEntity>> GetFieldBinding(SqlStorageField<TEntity> storageField)
		{
			return FieldBindings.FirstOrDefault(q => q.TargetField.Equals(storageField));
		}

		public ModelFieldBinding<PropertyField, SqlStorageField<TEntity>> GetFieldBinding(PropertyField propertyField)
		{
			return FieldBindings.FirstOrDefault(q => q.SourceField.Equals(propertyField));
		}
	}
}
