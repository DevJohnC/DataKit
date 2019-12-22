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
	public class GroupByBuilder<TEntity>
		where TEntity : class
	{
		private readonly ValueConverter<TEntity> _valueConverter;
		private readonly SqlDataModel<TEntity> _dataModel;
		private readonly DataSchema _dataSchema;
		private readonly IAliasIdentifier _tableIdentifier;
		private readonly List<SqlExpression<TEntity>> _expressions = new List<SqlExpression<TEntity>>();

		public GroupByBuilder(SqlDataModel<TEntity> dataModel, ValueConverter<TEntity> valueConverter)
		{
			_dataModel = dataModel;
			_dataSchema = dataModel.DataSchema;
			_valueConverter = valueConverter;
			_tableIdentifier = valueConverter.TableIdentifier;
		}

		public void GroupBy<TValue>(SqlValueExpression<TEntity, TValue> expression)
		{
			_expressions.Add(expression);
		}

		public void GroupBy<TValue>(Expression<Func<TEntity, TValue>> expression)
		{
			GroupBy(_valueConverter.ConvertExpression(expression));
		}

		public void GroupBy<TValue>(SqlStorageField<TEntity, TValue> field)
		{
			if (!field.RequiresJoin)
			{
				GroupBy(new SqlValueExpression<TEntity, TValue>(
					QueryExpression.Column(field.ColumnName),
					null
				));
				return;
			}

			var joins = field.JoinSpecification.CreateJoin(_dataSchema, field, _tableIdentifier)
				.Select(q => q.Builder).ToArray();
			GroupBy(new SqlValueExpression<TEntity, TValue>(
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
