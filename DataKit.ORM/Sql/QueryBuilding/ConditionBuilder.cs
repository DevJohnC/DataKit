using DataKit.ORM.Schema;
using DataKit.ORM.Schema.Sql;
using DataKit.ORM.Sql.Expressions;
using Silk.Data.SQL.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DataKit.ORM.Sql.QueryBuilding
{
	public class ConditionBuilder<TEntity>
		where TEntity : class
	{
		private readonly List<JoinBuilder<TEntity>> _joinBuilders = new List<JoinBuilder<TEntity>>();
		private readonly ConditionConverter<TEntity> _conditionConveter;
		private readonly SqlDataModel<TEntity> _dataModel;
		private readonly DataSchema _dataSchema;
		private readonly IAliasIdentifier _tableIdentifier;
		private QueryExpression _expression;

		public ConditionBuilder(SqlDataModel<TEntity> dataModel, ConditionConverter<TEntity> conditionConverter)
		{
			_dataModel = dataModel;
			_dataSchema = dataModel.DataSchema;
			_conditionConveter = conditionConverter;
			_tableIdentifier = conditionConverter.TableIdentifier;
		}

		private void AddJoins(JoinBuilder<TEntity>[] joinBuilders)
		{
			if (joinBuilders == null)
				return;

			foreach (var joinBuilder in joinBuilders)
				AddJoin(joinBuilder);
		}

		private void AddJoin(JoinBuilder<TEntity> joinBuilder)
		{
			if (joinBuilder == null || _joinBuilders.Any(q => q.Specification == joinBuilder.Specification))
				return;

			_joinBuilders.Add(joinBuilder);
		}

		public ConditionBuilder<TEntity> New()
			=> new ConditionBuilder<TEntity>(_dataModel, _conditionConveter);

		public void AndAlso(SqlValueExpression<TEntity, bool> conditionExpression)
		{
			_expression = QueryExpression.CombineConditions(
				_expression,
				ConditionType.AndAlso,
				conditionExpression.QueryExpression
				);
			AddJoins(conditionExpression.Joins);
		}

		public void AndAlso(Expression<Func<TEntity, bool>> conditionExpression)
			=> AndAlso(_conditionConveter.ConvertClause(conditionExpression));

		public void AndAlso<TValue>(SqlStorageField<TEntity, TValue> field, ComparisonOperator comparisonType, TValue value)
		{
			if (field.RequiresJoin)
			{
				foreach (var (joinBuilder, _) in field.JoinSpecification.CreateJoin(_dataSchema, field, _tableIdentifier))
					AddJoin(joinBuilder);
			}

			_expression = QueryExpression.CombineConditions(
				_expression,
				ConditionType.AndAlso,
				QueryExpression.Compare(
					QueryExpression.Column(field.ColumnName),
					comparisonType,
					QueryExpression.Value(value)
					));
		}

		public void OrElse(SqlValueExpression<TEntity, bool> conditionExpression)
		{
			_expression = QueryExpression.CombineConditions(
				_expression,
				ConditionType.OrElse,
				conditionExpression.QueryExpression
				);
			AddJoins(conditionExpression.Joins);
		}

		public void OrElse(Expression<Func<TEntity, bool>> conditionExpression)
			=> OrElse(_conditionConveter.ConvertClause(conditionExpression));

		public void OrElse<TValue>(SqlStorageField<TEntity, TValue> field, ComparisonOperator comparisonType, TValue value)
		{
			if (field.RequiresJoin)
			{
				foreach (var (joinBuilder, _) in field.JoinSpecification.CreateJoin(_dataSchema, field, _tableIdentifier))
					AddJoin(joinBuilder);
			}

			_expression = QueryExpression.CombineConditions(
				_expression,
				ConditionType.OrElse,
				QueryExpression.Compare(
					QueryExpression.Column(field.ColumnName),
					comparisonType,
					QueryExpression.Value(value)
					));
		}

		public SqlValueExpression<TEntity, bool> Build()
		{
			if (_expression == null)
				return null;
			return new SqlValueExpression<TEntity, bool>(_expression, _joinBuilders.ToArray());
		}
	}
}
