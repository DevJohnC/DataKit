using DataKit.Mapping;
using DataKit.Mapping.Binding;
using DataKit.Modelling.TypeModels;
using DataKit.ORM.Schema.Sql;
using DataKit.ORM.Sql.Expressions;
using Silk.Data.SQL.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataKit.ORM.Sql.QueryBuilding
{
	public abstract class ProjectionModel<TEntity>
		where TEntity : class
	{
		protected ProjectionModel(
			SqlDataModel<TEntity> dataModel, IReadOnlyList<ModelFieldBinding<SqlStorageField<TEntity>, PropertyField>> requiredFields,
			DataModelBinding<SqlStorageModel<TEntity>, SqlStorageField<TEntity>, TypeModel<TEntity>, PropertyField> storageToEntityBinding,
			Mapping<SqlStorageModel<TEntity>, SqlStorageField<TEntity>, TypeModel<TEntity>, PropertyField> storageToEntityMapping)
		{
			DataModel = dataModel ?? throw new ArgumentNullException(nameof(dataModel));
			RequiredFields = requiredFields ?? throw new ArgumentNullException(nameof(requiredFields));
			StorageToEntityBinding = storageToEntityBinding ?? throw new ArgumentNullException(nameof(storageToEntityBinding));
			StorageToEntityMapping = storageToEntityMapping ?? throw new ArgumentNullException(nameof(storageToEntityMapping));
		}

		public SqlDataModel<TEntity> DataModel { get; }

		public IReadOnlyList<ModelFieldBinding<SqlStorageField<TEntity>, PropertyField>> RequiredFields { get; }

		public DataModelBinding<SqlStorageModel<TEntity>, SqlStorageField<TEntity>, TypeModel<TEntity>, PropertyField> StorageToEntityBinding { get; }

		public Mapping<SqlStorageModel<TEntity>, SqlStorageField<TEntity>, TypeModel<TEntity>, PropertyField> StorageToEntityMapping { get; }
	}

	public class ProjectionModel<TEntity, TView> : ProjectionModel<TEntity>
		where TEntity : class
		where TView : class
	{
		public ProjectionModel(
			SqlDataModel<TEntity> dataModel,
			IReadOnlyList<ModelFieldBinding<SqlStorageField<TEntity>, PropertyField>> requiredFields,
			DataModelBinding<TypeModel<TEntity>, PropertyField, TypeModel<TView>, PropertyField> entityToViewBinding,
			Mapping<TypeModel<TEntity>, PropertyField, TypeModel<TView>, PropertyField> entityToViewMapping,
			DataModelBinding<SqlStorageModel<TEntity>, SqlStorageField<TEntity>, TypeModel<TEntity>, PropertyField> storageToEntityBinding,
			Mapping<SqlStorageModel<TEntity>, SqlStorageField<TEntity>, TypeModel<TEntity>, PropertyField> storageToEntityMapping) :
			base(dataModel, requiredFields, storageToEntityBinding, storageToEntityMapping)
		{
			EntityToViewBinding = entityToViewBinding ?? throw new ArgumentNullException(nameof(entityToViewBinding));
			EntityToViewMapping = entityToViewMapping ?? throw new ArgumentNullException(nameof(entityToViewMapping));
		}

		public DataModelBinding<TypeModel<TEntity>, PropertyField, TypeModel<TView>, PropertyField> EntityToViewBinding { get; }

		public Mapping<TypeModel<TEntity>, PropertyField, TypeModel<TView>, PropertyField> EntityToViewMapping { get; }

		public (IReadOnlyDictionary<string, AliasExpression> Fields, IReadOnlyList<JoinBuilder<TEntity>> RequiredJoins) GetProjectedFields(IAliasIdentifier tableIdentifier)
		{
			var aliasedFields = new Dictionary<string, AliasExpression>();
			var joinBuilders = new List<JoinBuilder<TEntity>>();
			var dataSchema = DataModel.DataSchema;

			foreach (var fieldBinding in RequiredFields)
			{
				var fieldAlias = fieldBinding.SourceField.FieldName;
				var columnName = fieldBinding.SourceField.ColumnName;

				if (aliasedFields.ContainsKey(fieldAlias))
					continue;

				var sourceIdentifier = tableIdentifier;

				if (fieldBinding.SourceField.RequiresJoin)
				{
					var joins = fieldBinding.SourceField.JoinSpecification.CreateJoin(
						dataSchema, fieldBinding.SourceField, tableIdentifier);

					foreach (var (joinBuilder, joinIdentifier) in joins)
					{
						joinBuilders.Add(joinBuilder);
						sourceIdentifier = joinIdentifier;
					}
				}

				aliasedFields.Add(
					fieldAlias,
					new AliasExpression(
						new ColumnWithAliasSourceQueryExpression(columnName, sourceIdentifier, fieldBinding.SourceField),
						fieldAlias)
				);
			}

			return (aliasedFields, joinBuilders);
		}

		public static ProjectionModel<TEntity, TView> Create(
			SqlDataModel<TEntity> dataModel,
			DataModelBinding<TypeModel<TEntity>, PropertyField, TypeModel<TView>, PropertyField> binding = null
			)
		{
			if (binding == null)
			{
				binding = new TypeBindingBuilder<TEntity, TView>()
					.AutoBind()
					.BuildBinding();
			}

			var entityBindings = new Dictionary<string, ModelFieldBinding<SqlStorageField<TEntity>, PropertyField>>();
			var requiredStorageFields = dataModel.GetRequiredStorageFields(binding).ToArray();
			foreach (var fieldBinding in requiredStorageFields)
			{
				var fieldAlias = fieldBinding.SourceField.FieldName;
				var columnName = fieldBinding.SourceField.ColumnName;

				if (entityBindings.ContainsKey(fieldAlias))
					continue;

				entityBindings.Add(fieldAlias, fieldBinding);
			}

			var storageToEntityBinding = new DataModelBinding<SqlStorageModel<TEntity>, SqlStorageField<TEntity>, TypeModel<TEntity>, PropertyField>(
				dataModel.StorageModel,
				binding.SourceModel,
				entityBindings.Values.ToArray()
				);

			return new ProjectionModel<TEntity, TView>(
				dataModel,
				requiredStorageFields,
				binding,
				binding.BuildMapping(),
				storageToEntityBinding,
				storageToEntityBinding.BuildMapping()
				);
		}
	}
}
