using DataKit.Mapping;
using DataKit.Mapping.Binding;
using DataKit.Modelling.TypeModels;
using DataKit.ORM.Schema;
using DataKit.ORM.Schema.Sql;
using DataKit.ORM.Sql.Expressions;
using DataKit.ORM.Sql.Mapping;
using Silk.Data.SQL.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DataKit.ORM.Sql.QueryBuilding
{
	public class ProjectionBuilder<TEntity>
		where TEntity : class
	{
		private readonly Dictionary<string, AliasExpression> _projectionExpressions
			= new Dictionary<string, AliasExpression>();
		private readonly List<JoinBuilder<TEntity>> _joinBuilders = new List<JoinBuilder<TEntity>>();
		private readonly ValueConverter<TEntity> _valueConverter;
		private readonly SqlDataModel<TEntity> _dataModel;
		private readonly DataSchema _dataSchema;
		private readonly IAliasIdentifier _tableIdentifier;
		private readonly IObjectFactory _objectFactory;

		public IReadOnlyList<JoinBuilder<TEntity>> Joins => _joinBuilders;
		public IEnumerable<AliasExpression> ProjectionExpressions => _projectionExpressions.Values;

		public ProjectionBuilder(SqlDataModel<TEntity> dataModel, ValueConverter<TEntity> valueConverter,
			IObjectFactory objectFactory = null)
		{
			_dataModel = dataModel;
			_dataSchema = dataModel.DataSchema;
			_valueConverter = valueConverter;
			_tableIdentifier = valueConverter.TableIdentifier;
			_objectFactory = objectFactory ?? DefaultObjectFactory.Instance;
		}

		public void Clear()
		{
			_projectionExpressions.Clear();
			_joinBuilders.Clear();
		}

		public IResultMapper<TView> Select<TView>(
			ProjectionModel<TEntity, TView> projectionModel
			) where TView : class
		{
			var (fields, joins) = projectionModel.GetProjectedFields(_tableIdentifier);
			_joinBuilders.AddRange(joins);
			foreach (var kvp in fields)
			{
				if (_projectionExpressions.ContainsKey(kvp.Key))
					continue;
				_projectionExpressions.Add(kvp.Key, kvp.Value);
			}

			return new EntityTypeMapper<TEntity, TView>(
				_dataModel.StorageModel,
				projectionModel.StorageToEntityMapping,
				projectionModel.EntityToViewMapping,
				_objectFactory
				);
		}

		public IResultMapper<T> Select<T>(SqlStorageField<TEntity, T> field)
		{
			var alias = $"__AutoAlias_{_projectionExpressions.Count}";
			_projectionExpressions.Add(
				alias,
				new AliasExpression(
					QueryExpression.Column(field.ColumnName),
					alias)
				);
			if (field.RequiresJoin)
			{
				_joinBuilders.AddRange(
					field.JoinSpecification.CreateJoin(_dataSchema, field, _tableIdentifier)
						.Select(q => q.Builder)
					);
			}
			return new ValueMapper<T>(alias);
		}

		public IResultMapper<T> Select<T>(Expression<Func<TEntity, T>> expression)
		{
			var sqlExpression = _valueConverter.ConvertExpression(expression);
			var alias = $"__AutoAlias_{_projectionExpressions.Count}";
			_projectionExpressions.Add(
				alias,
				new AliasExpression(
					sqlExpression.QueryExpression,
					alias)
				);
			if (sqlExpression.RequiresJoins)
				_joinBuilders.AddRange(sqlExpression.Joins);
			return new ValueMapper<T>(alias);
		}
	}
}
