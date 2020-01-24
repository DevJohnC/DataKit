using DataKit.Modelling;
using DataKit.Modelling.TypeModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataKit.ORM.Schema.Sql.Internal
{
	internal struct StorageModelConverter<TEntity> : IModelVisitor
		where TEntity : class
	{
		private readonly SqlEntityConfiguration _sqlEntityConfiguration;
		private readonly BuildContext _buildContext;
		private readonly Stack<IModelState> _modelStateStack;
		private readonly List<string> _currentPath;

		public StorageModelConverter(
			SqlEntityConfiguration sqlEntityConfiguration,
			BuildContext buildContext
			)
		{
			_currentPath = new List<string>();
			_sqlEntityConfiguration = sqlEntityConfiguration;
			_buildContext = buildContext;
			_modelStateStack = new Stack<IModelState>();
			_modelStateStack.Push(new ModelState<TEntity>(
				buildContext.SqlBuilders.First(q => q.EntityType == typeof(TEntity)),
				new string[0]
				));
		}

		public IModelNode VisitModel(IDataModel model)
		{
			var entityTypeModel = model as TypeModel;
			if (entityTypeModel == null)
				return null;

			var fields = new List<SqlStorageField<TEntity>>();
			foreach (var field in model.Fields)
			{
				_currentPath.Add(field.FieldName);

				var stateNode = (ModelStateNode)Visit(field);
				var sqlEntityField = stateNode.TransformedNode as SqlStorageField<TEntity>;
				if (sqlEntityField != null)
					fields.Add(sqlEntityField);

				if (stateNode.EntityModelState != null)
					_modelStateStack.Push(stateNode.EntityModelState);

				if (SqlTypeHelper.GetDataType(field.FieldType.Type) == null)
				{
					var subModel = Visit(field.FieldModel) as SqlStorageModel<TEntity>;
					fields.AddRange(subModel.Fields);
				}

				if (stateNode.EntityModelState != null)
					_modelStateStack.Pop();

				_currentPath.RemoveAt(_currentPath.Count - 1);
			}
			return new SqlStorageModel<TEntity>(fields, _sqlEntityConfiguration.DefaultTableName);
		}

		public IModelNode VisitField<T>(IModelFieldOf<T> field)
		{
			var currentPath = _currentPath;
			var propertyField = field as PropertyField;
			if (propertyField == null)
				return null;

			var fieldConfiguration = FindLocalFieldConfiguration<T>(currentPath);
			if (fieldConfiguration != null)
			{
				var localStorageNode = CreateLocalStorageField<T>(fieldConfiguration, currentPath, field);
				if (fieldConfiguration.Options.IsForeignKey)
				{
					var entityBuilder = _buildContext.SqlBuilders.FirstOrDefault(q => q.EntityType == typeof(T));
					if (entityBuilder != null)
					{
						return new ModelStateNode(localStorageNode, new ModelState<T>(entityBuilder, currentPath.ToArray(),
							localStorageNode as SqlStorageField<TEntity>));
					}
				}
				return new ModelStateNode(localStorageNode, null);
			}

			fieldConfiguration = FindForeignFieldConfiguration<T>(currentPath);
			if (fieldConfiguration != null)
			{
				var foreignStorageNode = _modelStateStack.Peek()
					.CreateForeignStorageField<T>(fieldConfiguration, currentPath, field);
				if (fieldConfiguration.Options.IsForeignKey)
				{
					var entityBuilder = _buildContext.SqlBuilders.FirstOrDefault(q => q.EntityType == typeof(T));
					return new ModelStateNode(foreignStorageNode, new ModelState<T>(entityBuilder, currentPath.ToArray(),
							foreignStorageNode as SqlStorageField<TEntity>));
				}
				return new ModelStateNode(foreignStorageNode, null);
			}

			return default(ModelStateNode);
		}

		private SqlFieldConfiguration<T> FindForeignFieldConfiguration<T>(IEnumerable<string> path)
		{
			var skipCount = _modelStateStack.Peek().BasePath.Count;
			return _modelStateStack.Peek().SqlFieldConfigurations
				.OfType<SqlFieldConfiguration<T>>()
				.FirstOrDefault(q => q.PropertyPath.Path.SequenceEqual(path.Skip(skipCount)));
		}

		private IModelNode CreateLocalStorageField<T>(SqlFieldConfiguration<T> fieldConfiguration, IEnumerable<string> path,
			IModelFieldOf<T> field)
		{
			if (fieldConfiguration.Options.IsForeignKey)
			{
				//  todo: the pattern employed here only permits one foreign key to entities
				//    to support more complex foreign key relationships this needs to be adjusted
				return new ForeignKeyVisitor<TEntity, T>(_currentPath, fieldConfiguration)
					.Visit(fieldConfiguration.Options.ForeignKeyFieldPath.Field);
			}

			return new SqlStorageField<TEntity, T>(
				string.Join("_", path), true, true,
				fieldConfiguration.Options.IsServerGenerated,
				fieldConfiguration.Options.ColumnName,
				fieldConfiguration.Options.DefaultValue,
				fieldConfiguration.Options.IsPrimaryKey,
				new FieldGraphPath<PropertyField>(path.ToArray(), field as PropertyField));
		}

		private SqlFieldConfiguration<T> FindLocalFieldConfiguration<T>(IEnumerable<string> path)
			=> _modelStateStack.Last().SqlFieldConfigurations
				.OfType<SqlFieldConfiguration<T>>()
				.FirstOrDefault(q => q.PropertyPath.Path.SequenceEqual(path));

		public IModelNode Visit(IModelNode modelNode)
		{
			return modelNode?.Accept(this);
		}

		public IModelNode VisitExtension(IModelNode node)
		{
			return node;
		}

		private interface IModelState
		{
			List<SqlFieldConfiguration> SqlFieldConfigurations { get; }

			IReadOnlyList<string> BasePath { get; }

			JoinSpecification<TEntity> JoinSpecification { get; }

			IModelNode CreateForeignStorageField<T>(SqlFieldConfiguration<T> fieldConfiguration, IEnumerable<string> path,
				IModelFieldOf<T> field);
		}

		private struct ModelState<T> : IModelState
		{
			public List<SqlFieldConfiguration> SqlFieldConfigurations { get; }

			public IReadOnlyList<string> BasePath { get; }

			public JoinSpecification<TEntity> JoinSpecification { get; }

			public ModelState(SqlDataModelBuilder modelBuilder, IReadOnlyList<string> basePath)
			{
				SqlFieldConfigurations = modelBuilder.ModelledFields;
				BasePath = basePath;
				JoinSpecification = null;
			}

			public ModelState(SqlDataModelBuilder modelBuilder, IReadOnlyList<string> basePath,
				SqlStorageField<TEntity> foreignKeyField)
			{
				SqlFieldConfigurations = modelBuilder.ModelledFields;
				BasePath = basePath;
				JoinSpecification = new JoinSpecification<TEntity>(foreignKeyField.JoinSpecification, typeof(T),
					$"_{string.Join("_", basePath)}", new JoinColumnPair(
					foreignKeyField.ColumnName,
					SqlFieldConfigurations.First(q => q.Options.IsPrimaryKey).Options.ColumnName
					));
			}

			public IModelNode CreateForeignStorageField<T1>(SqlFieldConfiguration<T1> fieldConfiguration, IEnumerable<string> path, IModelFieldOf<T1> field)
			{
				if (fieldConfiguration.Options.IsForeignKey)
				{
					//  todo: the pattern employed here only permits one foreign key to entities
					//    to support more complex foreign key relationships this needs to be adjusted
					return new ForeignKeyVisitor<TEntity, T1>(path.ToArray(), fieldConfiguration, JoinSpecification)
						.Visit(fieldConfiguration.Options.ForeignKeyFieldPath.Field);
				}

				if (fieldConfiguration.Options.IsPrimaryKey)
					return null;

				return new ForeignSqlStorageField<TEntity, T, T1>(
					string.Join("_", path), true, true,
					fieldConfiguration.Options.IsServerGenerated,
					fieldConfiguration.Options.ColumnName,
					fieldConfiguration.Options.DefaultValue,
					fieldConfiguration.Options.IsPrimaryKey,
					new FieldGraphPath<PropertyField>(path.ToArray(), field as PropertyField),
					JoinSpecification);
			}
		}

		private struct ModelStateNode : IModelNode
		{
			public ModelStateNode(IModelNode transformedNode, IModelState entityModelState)
			{
				TransformedNode = transformedNode;
				EntityModelState = entityModelState;
			}

			public IModelNode TransformedNode { get; }

			public IModelState EntityModelState { get; }

			public IModelNode Accept(IModelVisitor visitor)
				=> visitor?.VisitExtension(this);
		}
	}

	internal struct ForeignKeyVisitor<TEntity, TForeignEntity> : IModelVisitor
		where TEntity : class
	{
		private readonly IReadOnlyList<string> _currentPath;
		private readonly SqlFieldConfiguration<TForeignEntity> _sqlFieldConfiguration;
		private readonly JoinSpecification<TEntity> _joinSpecification;

		public ForeignKeyVisitor(IReadOnlyList<string> currentPath, SqlFieldConfiguration<TForeignEntity> sqlFieldConfiguration)
		{
			_currentPath = currentPath
				.Concat(sqlFieldConfiguration.Options.ForeignKeyFieldPath.Path)
				.ToArray();
			_sqlFieldConfiguration = sqlFieldConfiguration;
			_joinSpecification = null;
		}

		public ForeignKeyVisitor(IReadOnlyList<string> currentPath, SqlFieldConfiguration<TForeignEntity> sqlFieldConfiguration,
			JoinSpecification<TEntity> joinSpecification)
		{
			_currentPath = currentPath
				.Concat(sqlFieldConfiguration.Options.ForeignKeyFieldPath.Path)
				.ToArray();
			_sqlFieldConfiguration = sqlFieldConfiguration;
			_joinSpecification = joinSpecification;
		}

		public IModelNode Visit(IModelNode modelNode)
		{
			return modelNode?.Accept(this);
		}

		public IModelNode VisitExtension(IModelNode node)
		{
			throw new NotImplementedException();
		}

		public IModelNode VisitField<T>(IModelFieldOf<T> field)
		{
			var propertyField = field as PropertyField;
			if (propertyField == null)
				return null;

			if (_joinSpecification != null)
			{
				return new ForeignSqlStorageField<TEntity, TForeignEntity, T>(
					string.Join("_", _currentPath), true, true,
					_sqlFieldConfiguration.Options.IsServerGenerated,
					_sqlFieldConfiguration.Options.ColumnName,
					null,
					_sqlFieldConfiguration.Options.IsPrimaryKey,
					new FieldGraphPath<PropertyField>(_currentPath.ToArray(), propertyField),
					_joinSpecification);
			}

			return new ForeignKeySqlStorageField<TEntity, TForeignEntity, T>(
				string.Join("_", _currentPath), true, true,
				_sqlFieldConfiguration.Options.IsServerGenerated,
				_sqlFieldConfiguration.Options.ColumnName,
				null,
				_sqlFieldConfiguration.Options.IsPrimaryKey,
				new FieldGraphPath<PropertyField>(_currentPath.ToArray(), propertyField));
		}

		public IModelNode VisitModel(IDataModel model)
		{
			throw new NotImplementedException();
		}
	}
}
