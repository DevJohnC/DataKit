using System;
using System.Collections.Generic;
using System.Linq;
using DataKit.Mapping.Binding;
using DataKit.Modelling.TypeModels;

namespace DataKit.ORM.Schema.Sql
{
	public class OutBindingPairProvider<TEntity> : IBindingPairProvider<SqlStorageModel<TEntity>, SqlStorageField<TEntity>, TypeModel, PropertyField>
		where TEntity : class
	{
		public IEnumerable<BindingPair<SqlStorageField<TEntity>, PropertyField>> GetBindingPairs(SqlStorageModel<TEntity> sourceModel, TypeModel targetModel)
		{
			if (targetModel is SqlEntityModel<TEntity> entityModel)
				return GetBindingPairs(sourceModel, entityModel);

			throw new InvalidOperationException("Can only bind storage models to entity models.");
		}

		public IEnumerable<BindingPair<SqlStorageField<TEntity>, PropertyField>> GetBindingPairs(
			SqlStorageModel<TEntity> sourceModel,
			SqlEntityModel<TEntity> targetModel)
		{
			foreach (var targetField in targetModel.Fields)
			{
				var sourceField = sourceModel.Fields.FirstOrDefault(
					q => q.TypeModelGraphPath.Path.SequenceEqual(targetField.TypeModelGraphPath.Path)
					);
				if (sourceField != null)
				{
					var bindingSource = new ModelFieldBindingSource<SqlStorageField<TEntity>>(
						new[] { sourceField.FieldName },
						sourceField
						);
					yield return new BindingPair<SqlStorageField<TEntity>, PropertyField>(
						bindingSource,
						new ModelFieldBindingTarget<PropertyField>(targetField.TypeModelGraphPath.Path, targetField)
						);
				}
				else if (targetField.FieldModel != null && targetField.FieldModel.Fields.Count > 0)
				{
					foreach (var pair in GetBindingPairs(sourceModel, targetField.FieldModel))
						yield return pair;
				}
			}
		}
	}
}
