using DataKit.Modelling;
using DataKit.Modelling.TypeModels;
using DataKit.ORM.Schema.Sql.Internal;
using DataKit.ORM.Schema.Sql.Policies;
using System;
using System.Collections.Generic;

namespace DataKit.ORM.Schema.Sql
{
	public abstract class SqlDataModelBuilder
	{
		public List<SqlFieldConfiguration> ModelledFields { get; }
			= new List<SqlFieldConfiguration>();

		public abstract Type EntityType { get; }

		public abstract void DefineModelFields(BuildContext context);

		public abstract void DefineRelationshipFields(BuildContext context);

		public abstract SqlDataModel Build(BuildContext context);

		public static SqlDataModelBuilder Create(Type entityType, DataSchemaBuilder schemaBuilder, SqlEntityConfiguration entityConfiguration)
		{
			return Activator.CreateInstance(
				typeof(SqlDataModelBuilder<>).MakeGenericType(entityType),
				schemaBuilder,
				entityConfiguration
				) as SqlDataModelBuilder;
		}
	}

	public class SqlDataModelBuilder<TEntity> : SqlDataModelBuilder
		where TEntity : class
	{
		private readonly DataSchemaBuilder _schemaBuilder;
		private readonly SqlEntityConfiguration _entityConfiguration;

		public SqlDataModelBuilder(DataSchemaBuilder schemaBuilder, SqlEntityConfiguration entityConfiguration)
		{
			_schemaBuilder = schemaBuilder;
			_entityConfiguration = entityConfiguration;
		}

		public override Type EntityType => typeof(TEntity);

		public override void DefineModelFields(BuildContext context)
		{
			var modellingPolicies = _entityConfiguration.ModellingPolicies;
			if (modellingPolicies.Count == 0)
				modellingPolicies.AddDefaults();

			foreach (var modellingPolicy in modellingPolicies)
			{
				modellingPolicy.Apply(context, BuildStep.DefineFields, _entityConfiguration, this);
			}
		}

		public override void DefineRelationshipFields(BuildContext context)
		{
			var modellingPolicies = _entityConfiguration.ModellingPolicies;
			if (modellingPolicies.Count == 0)
				modellingPolicies.AddDefaults();

			foreach (var modellingPolicy in modellingPolicies)
			{
				modellingPolicy.Apply(context, BuildStep.DefineRelationships, _entityConfiguration, this);
			}
		}

		public override SqlDataModel Build(BuildContext context)
		{
			var entityTypeModel = TypeModel.GetModelOf<TEntity>();

			var entityModel = new EntityModelConverter(ModelledFields).Visit(entityTypeModel) as SqlEntityModel<TEntity>;
			var storageModel = new StorageModelConverter<TEntity>(_entityConfiguration, context)
				.Visit(entityTypeModel) as SqlStorageModel<TEntity>;
			var inBinding = new SqlInBindingBuilder<TEntity>(entityModel, storageModel)
				.AutoBind(new InBindingPairProvider<TEntity>())
				.BuildBinding();
			var outBinding = new SqlOutBindingBuilder<TEntity>(storageModel, entityModel)
				.AutoBind(new OutBindingPairProvider<TEntity>())
				.BuildBinding();

			return new SqlDataModel<TEntity>(
				entityModel, storageModel,
				inBinding, outBinding
				);
		}

		private struct EntityModelConverter : IModelVisitor
		{
			private readonly List<SqlFieldConfiguration> _sqlFieldConfigurations;
			private readonly List<string> _currentPath;

			public EntityModelConverter(
				List<SqlFieldConfiguration> sqlFieldConfigurations
				)
			{
				_currentPath = new List<string>();
				_sqlFieldConfigurations = sqlFieldConfigurations;
			}

			public IModelNode VisitModel(IDataModel model)
			{
				var fields = new List<SqlEntityField<TEntity>>();
				foreach (var field in model.Fields)
				{
					_currentPath.Add(field.FieldName);

					var sqlEntityField = Visit(field) as SqlEntityField<TEntity>;
					if (sqlEntityField != null)
					{
						var subModel = Visit(field.FieldModel);
						sqlEntityField.FieldModel = subModel as SqlEntityModel<TEntity>;
						fields.Add(sqlEntityField);
					}

					_currentPath.RemoveAt(_currentPath.Count - 1);
				}
				return new SqlEntityModel<TEntity>(fields);
			}

			public IModelNode VisitField<T>(IModelFieldOf<T> field)
			{
				var currentPath = _currentPath;
				var propertyField = field as PropertyField;
				if (propertyField == null)
					return null;

				return new SqlEntityField<TEntity, T>(
					field.FieldName, field.CanWrite, field.CanRead,
					propertyField.Property,
					new FieldGraphPath<PropertyField>(_currentPath.ToArray(), propertyField)
					);
			}

			public IModelNode Visit(IModelNode modelNode)
			{
				return modelNode?.Accept(this);
			}

			public IModelNode VisitExtension(IModelNode node)
			{
				return node;
			}
		}
	}
}
