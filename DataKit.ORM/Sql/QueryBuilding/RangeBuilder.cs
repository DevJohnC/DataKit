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
	public class RangeBuilder<TEntity>
		where TEntity : class
	{
		private readonly ValueConverter<TEntity> _valueConverter;
		private readonly SqlDataModel<TEntity> _dataModel;
		private readonly DataSchema _dataSchema;
		private readonly IAliasIdentifier _tableIdentifier;

		private SqlExpression<TEntity> _limit;

		private SqlExpression<TEntity> _offset;

		public RangeBuilder(SqlDataModel<TEntity> dataModel, ValueConverter<TEntity> valueConverter)
		{
			_dataModel = dataModel;
			_dataSchema = dataModel.DataSchema;
			_valueConverter = valueConverter;
			_tableIdentifier = valueConverter.TableIdentifier;
		}

		private IEnumerable<JoinBuilder<TEntity>> GetJoins()
		{
			if (_limit?.RequiresJoins == true)
			{
				foreach (var join in _limit.Joins)
					yield return join;
			}

			if (_offset?.RequiresJoins == true)
			{
				foreach (var join in _offset.Joins)
					yield return join;
			}
		}

		public SqlExpression<TEntity> Build()
		{
			if (_limit != null)
			{
				if (_offset == null)
				{
					//  limit only
					return new SqlExpression<TEntity>(
						QueryExpression.Limit(_limit.QueryExpression),
						_limit.Joins
					);
				}
				else
				{
					//  limit and offset
					return new SqlExpression<TEntity>(
						QueryExpression.Limit(_limit.QueryExpression, _offset.QueryExpression),
						GetJoins().ToArray()
					);
				}
			}
			else if (_offset != null)
			{
				//  offset without a limit
				return new SqlExpression<TEntity>(
					QueryExpression.Limit(QueryExpression.Value(int.MaxValue), _offset.QueryExpression),
					_offset.Joins
					);
			}
			return null;
		}

		public void Limit<TValue>(SqlValueExpression<TEntity, TValue> expression)
		{
			_limit = expression;
		}

		public void Limit(int limit)
		{
			Limit(new SqlValueExpression<TEntity, int>(
				ORMQueryExpressions.Value(limit),
				new JoinBuilder<TEntity>[0]
				));
		}

		public void Limit<TValue>(Expression<Func<TEntity, TValue>> expression)
		{
			Limit(_valueConverter.ConvertExpression(expression));
		}

		public void Limit<TValue>(SqlStorageField<TEntity, TValue> field)
		{
			//  todo: support multiple nested JOINs
			if (!field.RequiresJoin)
			{
				Limit(new SqlValueExpression<TEntity, TValue>(
					QueryExpression.Column(field.ColumnName),
					null
				));
				return;
			}

			var joins = field.JoinSpecification.CreateJoin(_dataSchema, field, _tableIdentifier)
				.Select(q => q.Builder).ToArray();
			Limit(new SqlValueExpression<TEntity, TValue>(
				QueryExpression.Column(field.ColumnName),
				joins
			));
		}

		public void Offset<TValue>(SqlValueExpression<TEntity, TValue> expression)
		{
			_offset = expression;
		}

		public void Offset(int offset)
		{
			Offset(new SqlValueExpression<TEntity, int>(
				ORMQueryExpressions.Value(offset),
				new JoinBuilder<TEntity>[0]
				));
		}

		public void Offset<TValue>(Expression<Func<TEntity, TValue>> expression)
		{
			Offset(_valueConverter.ConvertExpression(expression));
		}

		public void Offset<TValue>(SqlStorageField<TEntity, TValue> field)
		{
			//  todo: support multiple nested JOINs
			if (!field.RequiresJoin)
			{
				Offset(new SqlValueExpression<TEntity, TValue>(
					QueryExpression.Column(field.ColumnName),
					null
				));
				return;
			}

			var joins = field.JoinSpecification.CreateJoin(_dataSchema, field, _tableIdentifier)
				.Select(q => q.Builder).ToArray();
			Offset(new SqlValueExpression<TEntity, TValue>(
				QueryExpression.Column(field.ColumnName),
				joins
			));
		}
	}
}
