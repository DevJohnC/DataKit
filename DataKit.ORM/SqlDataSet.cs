using System;
using System.Linq;
using System.Linq.Expressions;
using DataKit.Mapping;
using DataKit.Mapping.Binding;
using DataKit.Modelling;
using DataKit.Modelling.TypeModels;
using DataKit.ORM.Schema.Sql;
using DataKit.ORM.Sql;
using DataKit.ORM.Sql.QueryBuilding;
using Silk.Data.SQL.Expressions;
using IQueryProvider = Silk.Data.SQL.Providers.IQueryProvider;

namespace DataKit.ORM
{
	public abstract class SqlDataSet : DataSet
	{
		protected SqlDataSet(SqlDataModel dataModel, IQueryProvider queryProvider)
		{
			DataModel = dataModel ?? throw new ArgumentNullException(nameof(dataModel));
			QueryProvider = queryProvider ?? throw new ArgumentNullException(nameof(queryProvider));
		}

		public SqlDataSetBindings Bindings { get; protected set; }

		public SqlDataModel DataModel { get; }

		public IQueryProvider QueryProvider { get; }

		public SqlServerFunctions ServerFunctions { get; }
	}

	public class SqlDataSet<TEntity> : SqlDataSet
		where TEntity : class
	{
		private readonly IObjectFactory _objectFactory;

		private SqlDataSetBindings<TEntity> _bindings;
		public new SqlDataSetBindings<TEntity> Bindings
		{
			get => _bindings;
			protected set
			{
				_bindings = value;
				base.Bindings = value;
			}
		}

		public new SqlDataModel<TEntity> DataModel { get; }

		private readonly PrimaryKeyHelper[] _primaryKeyHelpers;

		public SqlDataSet(SqlDataModel<TEntity> dataModel, IQueryProvider queryProvider, IObjectFactory objectFactory = null) :
			base(dataModel, queryProvider)
		{
			_objectFactory = objectFactory ?? DefaultObjectFactory.Instance;
			DataModel = dataModel;
			Bindings = new SqlDataSetBindings<TEntity>(dataModel);

			_primaryKeyHelpers = dataModel.InBinding.FieldBindings
				.Where(q => q.BindingTarget.Field.IsPrimaryKey)
				.Select(q => PrimaryKeyHelper.Create(q))
				.ToArray();
		}

		public SqlSelectOperation<TEntity, TEntity> Select()
		{
			return new SqlSelectOperation<TEntity, TEntity>(this, QueryProvider, _objectFactory)
				.Bind<TEntity>();
		}

		public SqlSelectOperation<TEntity, TView> Select<TView>(
			DataModelBinding<TypeModel<TEntity>, PropertyField, TypeModel<TView>, PropertyField> binding = null
			)
			where TView : class
		{
			return new SqlSelectOperation<TEntity, TView>(this, QueryProvider, _objectFactory)
				.Bind(binding);
		}

		public SqlSelectOperation<TEntity, TExpr> Select<TExpr>(Expression<Func<TEntity, TExpr>> expression)
		{
			return new SqlSelectOperation<TEntity, TExpr>(this, QueryProvider, _objectFactory)
				.Bind(expression);
		}

		public SqlInsertOperation<TEntity> Insert()
		{
			return new SqlInsertOperation<TEntity>(this, QueryProvider);
		}

		public SqlInsertOperation<TEntity> Insert(TEntity entity)
		{
			return Insert().Set(entity);
		}

		public SqlInsertOperation<TEntity> Insert<TView>(TView viewOfEntity,
			DataModelBinding<TypeModel<TView>, PropertyField, TypeModel<TEntity>, PropertyField> binding = null
			)
			where TView : class
		{
			if (binding == null)
			{
				binding = Bindings.BindViewToEntity<TView>();
			}
			return Insert().Set(viewOfEntity, binding);
		}

		public SqlInsertOperation<TEntity> Insert<TView>(TView viewOfEntity,
			Action<TypeBindingBuilder<TView, TEntity>> bind
			)
			where TView : class
		{
			var builder = new TypeBindingBuilder<TView, TEntity>();
			bind(builder);
			return Insert().Set(viewOfEntity, builder.BuildBinding());
		}

		public SqlUpdateOperation<TEntity> Update()
		{
			return new SqlUpdateOperation<TEntity>(this, QueryProvider);
		}

		public SqlUpdateOperation<TEntity> Update(TEntity entity)
		{
			var operation = Update().Set(entity);
			WriteEntityPrimaryKeyClause(entity, operation);
			return operation;
		}

		public SqlUpdateOperation<TEntity> Update<TView>(TView viewOfEntity,
			DataModelBinding<TypeModel<TView>, PropertyField, TypeModel<TEntity>, PropertyField> binding = null
			)
			where TView : class
		{
			if (binding == null)
			{
				binding = Bindings.BindViewToEntity<TView>();
			}
			return Update().Set(viewOfEntity, binding);
		}

		public SqlUpdateOperation<TEntity> Update<TView>(TView viewOfEntity,
			Action<TypeBindingBuilder<TView, TEntity>> bind
			)
			where TView : class
		{
			var builder = new TypeBindingBuilder<TView, TEntity>();
			bind(builder);
			return Update().Set(viewOfEntity, builder.BuildBinding());
		}

		public SqlDeleteOperation<TEntity> Delete()
		{
			return new SqlDeleteOperation<TEntity>(this, QueryProvider);
		}

		public SqlDeleteOperation<TEntity> Delete(TEntity entity)
		{
			var operation = Delete();
			WriteEntityPrimaryKeyClause(entity, operation);
			return operation;
		}

		protected void EnsureHasPrimaryKey()
		{
			if (_primaryKeyHelpers == null || _primaryKeyHelpers.Length == 0)
				throw new InvalidOperationException("Operation requires a primary key field.");
		}

		protected void WriteEntityPrimaryKeyClause(TEntity entity, IWhereQueryBuilder<TEntity> queryBuilder)
		{
			EnsureHasPrimaryKey();
			var nestedEntityClause = queryBuilder.WhereCondition.New();
			var entityReader = new ObjectDataModelReader<TEntity>(entity);
			var bindingContext = new BindingContext();
			foreach (var primaryKeyHelper in _primaryKeyHelpers)
			{
				primaryKeyHelper.WriteEntityPrimaryKeyClause(entityReader, nestedEntityClause, bindingContext);
			}
			queryBuilder.AndWhere(nestedEntityClause.Build());
		}

		protected abstract class PrimaryKeyHelper
		{
			public abstract void WriteEntityPrimaryKeyClause(IDataModelReader<TypeModel<TEntity>, PropertyField> entityReader, ConditionBuilder<TEntity> conditionBuilder, BindingContext bindingContext);

			public static PrimaryKeyHelper Create(ModelFieldBinding<PropertyField, SqlStorageField<TEntity>> fieldBinding)
			{
				return new CreationVisitor(fieldBinding).Create();
			}

			private struct CreationVisitor : IModelVisitor
			{
				private readonly ModelFieldBinding<PropertyField, SqlStorageField<TEntity>> _fieldBinding;

				public CreationVisitor(ModelFieldBinding<PropertyField, SqlStorageField<TEntity>> fieldBinding)
				{
					_fieldBinding = fieldBinding;
				}

				public PrimaryKeyHelper Create()
				{
					return ((PackedKeyHelper)Visit(_fieldBinding.TargetField)).PrimaryKeyHelper;
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
					return new PackedKeyHelper(
						new PrimaryKeyHelper<T>(_fieldBinding.BindingSource.Field as SqlEntityField<TEntity>,
						_fieldBinding.BindingTarget.Field as SqlStorageField<TEntity, T>,
						_fieldBinding.Transformation));
				}

				public IModelNode VisitModel(IDataModel model)
				{
					throw new NotImplementedException();
				}

				private struct PackedKeyHelper : IModelNode
				{
					public PackedKeyHelper(PrimaryKeyHelper primaryKeyHelper)
					{
						PrimaryKeyHelper = primaryKeyHelper;
					}

					public PrimaryKeyHelper PrimaryKeyHelper { get; }

					public IModelNode Accept(IModelVisitor visitor)
					{
						return visitor.VisitExtension(this);
					}
				}
			}
		}

		protected class PrimaryKeyHelper<TStoreType> : PrimaryKeyHelper
		{
			private readonly SqlEntityField<TEntity> _entityField;
			private readonly SqlStorageField<TEntity, TStoreType> _storageField;
			private readonly BindingTransformation _transformation;

			public PrimaryKeyHelper(SqlEntityField<TEntity> entityField, SqlStorageField<TEntity, TStoreType> storageField,
				BindingTransformation bindingTransformation)
			{
				_entityField = entityField;
				_storageField = storageField;
				_transformation = bindingTransformation;
			}

			public override void WriteEntityPrimaryKeyClause(IDataModelReader<TypeModel<TEntity>, PropertyField> entityReader, ConditionBuilder<TEntity> conditionBuilder, BindingContext bindingContext)
			{
				var entityValue = _entityField.ReadValue(entityReader);
				if (!_transformation(bindingContext, entityValue, out var storeValue))
					throw new Exception("Failed to read primary key field.");

				conditionBuilder.AndAlso(_storageField, ComparisonOperator.AreEqual, (TStoreType)storeValue);
			}
		}
	}

	public class SqlDataSet<TBusiness, TEntity> : SqlDataSet<TEntity>
		where TEntity : class
		where TBusiness : class
	{
		private SqlDataSetBindings<TBusiness, TEntity> _bindings;
		public new SqlDataSetBindings<TBusiness, TEntity> Bindings
		{
			get => _bindings;
			protected set
			{
				_bindings = value;
				base.Bindings = value;
			}
		}

		public SqlDataSet(SqlDataModel<TEntity> dataModel, IQueryProvider queryProvider,
			IObjectFactory objectFactory = null,
			DataModelBinding<TypeModel<TBusiness>, PropertyField, TypeModel<TEntity>, PropertyField> businessToEntityBinding = null,
			DataModelBinding<TypeModel<TEntity>, PropertyField, TypeModel<TBusiness>, PropertyField> entityToBusinessBinding = null) :
			base(dataModel, queryProvider, objectFactory)
		{
			Bindings = new SqlDataSetBindings<TBusiness, TEntity>(dataModel, businessToEntityBinding, entityToBusinessBinding);
		}
	}
}
