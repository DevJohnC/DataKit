using DataKit.Mapping.Binding;
using DataKit.Modelling;
using DataKit.Modelling.TypeModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataKit.ORM.Schema.Sql
{
	public class InBindingPairProvider<TEntity> : IBindingPairProvider<TypeModel, PropertyField, SqlStorageModel<TEntity>, SqlStorageField<TEntity>>
		where TEntity : class
	{
		public IEnumerable<BindingPair<PropertyField, SqlStorageField<TEntity>>> GetBindingPairs(
			TypeModel sourceModel,
			SqlStorageModel<TEntity> targetModel)
		{
			if (sourceModel is SqlEntityModel<TEntity> entityModel)
				return GetBindingPairs(entityModel, targetModel);

			throw new InvalidOperationException("Can only bind entity models to storage models.");
		}

		public IEnumerable<BindingPair<PropertyField, SqlStorageField<TEntity>>> GetBindingPairs(
			SqlEntityModel<TEntity> sourceModel,
			SqlStorageModel<TEntity> targetModel)
		{
			foreach (var targetField in targetModel.Fields)
			{
				var sourceField = ResolveSourceField(sourceModel, targetField.TypeModelGraphPath);
				if (sourceField != null)
				{
					yield return new BindingPair<PropertyField, SqlStorageField<TEntity>>(
						sourceField,
						new ModelFieldBindingTarget<SqlStorageField<TEntity>>(new[] { targetField.FieldName }, targetField)
						);
				}
			}
		}

		private ModelFieldBindingSource<PropertyField> ResolveSourceField(
			SqlEntityModel<TEntity> sourceModel,
			FieldGraphPath<PropertyField> graphPath
			)
		{
			return Search(sourceModel.Fields, new string[0]);

			ModelFieldBindingSource<PropertyField> Search(
				IEnumerable<SqlEntityField<TEntity>> fields,
				string[] parentPath
				)
			{
				foreach (var field in fields)
				{
					var fieldPath = parentPath.Concat(new[] { field.FieldName }).ToArray();

					if (field.TypeModelGraphPath.Equals(graphPath))
						return new ModelFieldBindingSource<PropertyField>(
							fieldPath, field
							);

					if (field.FieldModel != null && field.FieldModel.Fields.Count > 0)
					{
						var subSearchResult = Search(field.FieldModel.Fields, fieldPath);
						if (subSearchResult != null)
							return subSearchResult;
					}
				}

				return null;
			}
		}
	}
}
