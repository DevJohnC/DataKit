using DataKit.Modelling;
using DataKit.Modelling.TypeModels;
using DataKit.ORM.Schema.Sql.Policies;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DataKit.ORM.Schema.Sql
{
	public class SqlEntityConfiguration
	{
		public Type EntityType { get; }

		public Type BusinessType { get; }

		public List<ISqlModellingPolicy> ModellingPolicies { get; }
			= new List<ISqlModellingPolicy>();

		public string DefaultTableName { get; set; }

		public SqlEntityConfiguration(Type businessType, Type entityType)
		{
			EntityType = entityType;
			BusinessType = businessType;
			DefaultTableName = EntityType.Name;
		}
	}

	public class SqlEntityConfiguration<TEntity> : SqlEntityConfiguration
		where TEntity : class
	{
		protected readonly TypeModel<TEntity> _entityTypeModel = TypeModel.GetModelOf<TEntity>();

		public SqlEntityConfiguration() : this(null)
		{
		}

		protected SqlEntityConfiguration(Type businessType) : base(businessType, typeof(TEntity))
		{
		}

		public SqlEntityConfiguration<TEntity> Field<T>(
			Expression<Func<TEntity, T>> propertySelector,
			Action<SqlFieldConfiguration<T>> configure = null
			)
		{
			var forField = _entityTypeModel.GetField(propertySelector);
			if (forField == null)
				throw new InvalidOperationException("Invalid field specified.");

			var fieldConfig = new SqlFieldConfiguration<T>()
				.ColumnName(forField.Field.FieldName)
				.DataType(SqlTypeHelper.GetDataType(forField.Field.FieldType.Type))
				.IsNullable(SqlTypeHelper.TypeIsNullable(forField.Field.FieldType.Type));
			fieldConfig.PropertyPath = new FieldGraphPath<PropertyField>(
				forField.Path,
				forField.Field
				);

			configure?.Invoke(fieldConfig);

			ModellingPolicies.Add(new ConfiguredFieldPolicy(fieldConfig));

			return this;
		}

		public SqlEntityConfiguration<TEntity> AutoModel()
		{
			ModellingPolicies.AddDefaults();
			return this;
		}
	}

	public class SqlEntityConfiguration<TBusiness, TEntity> : SqlEntityConfiguration<TEntity>
		where TBusiness : class
		where TEntity : class
	{
		public SqlEntityConfiguration() : base(typeof(TBusiness))
		{
		}
	}
}
