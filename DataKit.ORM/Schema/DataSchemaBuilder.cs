using DataKit.ORM.Schema.Sql;
using DataKit.ORM.Sql.Expressions.MethodConversion;
using System;
using System.Collections.Generic;

namespace DataKit.ORM.Schema
{
	/// <summary>
	/// Builds a data schema.
	/// </summary>
	public class DataSchemaBuilder
	{
		private readonly Dictionary<Type, SqlEntityConfiguration> _sqlEntityConfigurations
			= new Dictionary<Type, SqlEntityConfiguration>();

		private readonly List<ISqlMethodCallConverter> _sqlMethodCallConverters
			= new List<ISqlMethodCallConverter>();

		public DataSchemaBuilder AddSqlMethodCallConverter(ISqlMethodCallConverter sqlMethodCallConverter)
		{
			_sqlMethodCallConverters.Add(sqlMethodCallConverter);
			return this;
		}

		public DataSchemaBuilder ConfigureSqlEntity<TEntity>(Action<SqlEntityConfiguration<TEntity>> configure)
			where TEntity : class
		{
			if (!_sqlEntityConfigurations.TryGetValue(typeof(TEntity), out var sqlEntityConfig))
				throw new InvalidOperationException($"Entity type {typeof(TEntity).FullName} isn't registered.");

			configure(sqlEntityConfig as SqlEntityConfiguration<TEntity>);

			return this;
		}

		public DataSchemaBuilder AddSqlEntity(SqlEntityConfiguration entityConfiguration)
		{
			var entityType = entityConfiguration.EntityType;
			if (_sqlEntityConfigurations.ContainsKey(entityType))
				throw new InvalidOperationException("Entity type already registered.");

			_sqlEntityConfigurations.Add(entityType, entityConfiguration);

			return this;
		}

		public DataSchemaBuilder AddSqlEntity<TEntity>(
			Action<SqlEntityConfiguration<TEntity>> configure = null)
			where TEntity : class
		{
			var entityType = typeof(TEntity);
			if (_sqlEntityConfigurations.ContainsKey(entityType))
				throw new InvalidOperationException("Entity type already registered.");

			var configuration = new SqlEntityConfiguration<TEntity>();
			configure?.Invoke(configuration);
			_sqlEntityConfigurations.Add(entityType, configuration);

			return this;
		}

		public DataSchemaBuilder AddSqlEntity<TBusiness, TEntity>(
			Action<SqlEntityConfiguration<TBusiness, TEntity>> configure = null)
			where TBusiness : class
			where TEntity : class
		{
			var entityType = typeof(TEntity);
			if (_sqlEntityConfigurations.ContainsKey(entityType))
				throw new InvalidOperationException("Entity type already registered.");

			var configuration = new SqlEntityConfiguration<TBusiness, TEntity>();
			configure?.Invoke(configuration);
			_sqlEntityConfigurations.Add(entityType, configuration);

			return this;
		}

		public DataSchema Build()
		{
			var sqlDataModels = new List<SqlDataModel>();
			var buildCtx = new BuildContext();

			foreach (var kvp in _sqlEntityConfigurations)
			{
				buildCtx.SqlBuilders.Add(
					SqlDataModelBuilder.Create(kvp.Key, this, kvp.Value)
					);
			}

			foreach (var builder in buildCtx.SqlBuilders)
			{
				builder.DefineModelFields(buildCtx);
			}

			foreach (var builder in buildCtx.SqlBuilders)
			{
				builder.DefineRelationshipFields(buildCtx);
			}

			foreach (var builder in buildCtx.SqlBuilders)
			{
				sqlDataModels.Add(builder.Build(buildCtx));
			}

			return new DataSchema(
				new SqlSchema(sqlDataModels, _sqlMethodCallConverters)
				);
		}
	}
}
