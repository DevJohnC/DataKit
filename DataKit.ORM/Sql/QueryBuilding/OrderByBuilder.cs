using DataKit.ORM.Schema;
using DataKit.ORM.Schema.Sql;
using DataKit.ORM.Sql.Expressions;
using DataKit.SQL.QueryExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DataKit.ORM.Sql.QueryBuilding
{
	public class OrderByBuilder<TEntity>
		where TEntity : class
	{
		private readonly ValueConverter<TEntity> _valueConverter;
		private readonly SqlDataModel<TEntity> _dataModel;
		private readonly DataSchema _dataSchema;
		private readonly IAliasIdentifier _tableIdentifier;
		private readonly List<SqlExpression<TEntity>> _expressions = new List<SqlExpression<TEntity>>();

		public OrderByBuilder(SqlDataModel<TEntity> dataModel, ValueConverter<TEntity> valueConverter)
		{
			_dataModel = dataModel;
			_dataSchema = dataModel.DataSchema;
			_valueConverter = valueConverter;
			_tableIdentifier = valueConverter.TableIdentifier;
		}

		public void OrderBy<TValue>(SqlValueExpression<TEntity, TValue> expression)
		{
			_expressions.Add(expression);
		}

		public void OrderBy<TValue>(Expression<Func<TEntity, TValue>> expression)
		{
			OrderBy(_valueConverter.ConvertExpression(expression));
		}

		public void OrderBy<TValue>(SqlStorageField<TEntity, TValue> field)
		{
			//  todo: support multiple nested JOINs
			if (!field.RequiresJoin)
			{
				OrderBy(new SqlValueExpression<TEntity, TValue>(
					QueryExpression.Column(field.ColumnName),
					null
				));
				return;
			}

			var joins = field.JoinSpecification.CreateJoin(_dataSchema, field, _tableIdentifier)
				.Select(q => q.Builder).ToArray();
			OrderBy(new SqlValueExpression<TEntity, TValue>(
				QueryExpression.Column(field.ColumnName),
				joins
			));
		}

		public void OrderByDescending<TValue>(SqlValueExpression<TEntity, TValue> expression)
		{
			_expressions.Add(new SqlExpression<TEntity>(
				QueryExpression.Descending(expression.QueryExpression),
				expression.Joins
				));
		}

		public void OrderByDescending<TValue>(Expression<Func<TEntity, TValue>> expression)
		{
			OrderByDescending(_valueConverter.ConvertExpression(expression));
		}

		public void OrderByDescending<TValue>(SqlStorageField<TEntity, TValue> field)
		{
			//  todo: support multiple nested JOINs
			if (!field.RequiresJoin)
			{
				OrderByDescending(new SqlValueExpression<TEntity, TValue>(
					QueryExpression.Column(field.ColumnName),
					null
				));
				return;
			}

			var joins = field.JoinSpecification.CreateJoin(_dataSchema, field, _tableIdentifier)
				.Select(q => q.Builder).ToArray();
			OrderByDescending(new SqlValueExpression<TEntity, TValue>(
				QueryExpression.Column(field.ColumnName),
				joins
			));
		}

		public IReadOnlyList<SqlExpression<TEntity>> Build()
		{
			return _expressions;
		}
	}
}
