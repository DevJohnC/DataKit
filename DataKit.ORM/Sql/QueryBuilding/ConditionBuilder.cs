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
			_expression = _expression.AndAlso(
				conditionExpression.QueryExpression
				);
			AddJoins(conditionExpression.Joins);
		}

		public void AndAlso(Expression<Func<TEntity, bool>> conditionExpression)
			=> AndAlso(_conditionConveter.ConvertClause(conditionExpression));

		public void AndAlso<TValue>(SqlStorageField<TEntity, TValue> field, SqlComparisonOperator comparisonType, TValue value)
		{
			if (field.RequiresJoin)
			{
				foreach (var (joinBuilder, _) in field.JoinSpecification.CreateJoin(_dataSchema, field, _tableIdentifier))
					AddJoin(joinBuilder);
			}

			_expression = _expression.AndAlso(
				CreateComparison(
					QueryExpression.Column(field.ColumnName),
					comparisonType,
					QueryExpression.Value(value)
					));
		}

		public void OrElse(SqlValueExpression<TEntity, bool> conditionExpression)
		{
			_expression = _expression.OrElse(
				conditionExpression.QueryExpression
				);
			AddJoins(conditionExpression.Joins);
		}

		public void OrElse(Expression<Func<TEntity, bool>> conditionExpression)
			=> OrElse(_conditionConveter.ConvertClause(conditionExpression));

		public void OrElse<TValue>(SqlStorageField<TEntity, TValue> field, SqlComparisonOperator comparisonType, TValue value)
		{
			if (field.RequiresJoin)
			{
				foreach (var (joinBuilder, _) in field.JoinSpecification.CreateJoin(_dataSchema, field, _tableIdentifier))
					AddJoin(joinBuilder);
			}

			_expression = _expression.OrElse(
				CreateComparison(
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

		private static QueryExpression CreateComparison(QueryExpression left, SqlComparisonOperator comparisonType, QueryExpression right)
		{
			switch (comparisonType)
			{
				case SqlComparisonOperator.AreEqual:
					return QueryExpression.AreEqual(left, right);
				case SqlComparisonOperator.AreNotEqual:
					return QueryExpression.AreNotEqual(left, right);
				case SqlComparisonOperator.GreaterThan:
					return QueryExpression.GreaterThan(left, right);
				case SqlComparisonOperator.GreaterThanOrEqualTo:
					return QueryExpression.GreaterThanOrEqualTo(left, right);
				case SqlComparisonOperator.LessThan:
					return QueryExpression.LessThan(left, right);
				case SqlComparisonOperator.LessThanOrEqualTo:
					return QueryExpression.LessThanOrEqualTo(left, right);
				case SqlComparisonOperator.Like:
					return QueryExpression.Like(left, right);
			}
			throw new InvalidOperationException();
		}
	}
}
