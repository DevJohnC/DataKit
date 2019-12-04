using DataKit.ORM.Sql.QueryBuilding;
using Silk.Data.SQL.Expressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DataKit.ORM.Sql.Expressions.MethodConversion
{
	public class BuiltInMethodCallConverter : ISqlMethodCallConverter
	{
		private MethodInfo _lastInsertIdMethod = typeof(SqlServerFunctionsExtensions)
			.GetMethod(nameof(SqlServerFunctionsExtensions.LastInsertId));
		private MethodInfo _randomMethod = typeof(SqlServerFunctionsExtensions)
			.GetMethod(nameof(SqlServerFunctionsExtensions.Random));

		private MethodInfo[] _countMethods =
			typeof(SqlServerFunctionsExtensions).GetMethods()
				.Where(q => q.Name == nameof(SqlServerFunctionsExtensions.Count))
				.ToArray();
		private MethodInfo _isLikeMethod =
			typeof(SqlServerFunctionsExtensions).GetMethod(nameof(SqlServerFunctionsExtensions.IsLike));
		private MethodInfo[] _isInMethods =
			typeof(SqlServerFunctionsExtensions).GetMethods()
				.Where(q => q.Name == nameof(SqlServerFunctionsExtensions.IsIn))
				.ToArray();
		private MethodInfo _hasFlagMethod =
			typeof(SqlServerFunctionsExtensions).GetMethod(nameof(SqlServerFunctionsExtensions.HasFlag));
		private MethodInfo[] _minMethods =
			typeof(SqlServerFunctionsExtensions).GetMethods()
				.Where(q => q.Name == nameof(SqlServerFunctionsExtensions.Min))
				.ToArray();
		private MethodInfo[] _maxMethods =
			typeof(SqlServerFunctionsExtensions).GetMethods()
				.Where(q => q.Name == nameof(SqlServerFunctionsExtensions.Max))
				.ToArray();

		public bool TryConvertMethod<TEntity>(MethodCallExpression methodCallExpression,
			ExpressionConversionVisitor<TEntity> expressionConverter,
			out LinqQueryExpression<TEntity> convertedExpression)
			where TEntity : class
		{
			if (methodCallExpression.Method.IsGenericMethod)
			{
				var openGenericMethod = methodCallExpression.Method.GetGenericMethodDefinition();
				if (_hasFlagMethod == openGenericMethod)
				{
					convertedExpression = HasFlagCall<TEntity>(methodCallExpression, expressionConverter);
					return true;
				}
				else if (_isInMethods.Contains(openGenericMethod))
				{
					convertedExpression = IsInCall<TEntity>(methodCallExpression, expressionConverter);
					return true;
				}
			}
			else
			{
				if (methodCallExpression.Method == _lastInsertIdMethod)
				{
					convertedExpression = LastInsertIdCall<TEntity>();
					return true;
				}
				else if (methodCallExpression.Method == _randomMethod)
				{
					convertedExpression = RandomCall<TEntity>();
					return true;
				}
				if (methodCallExpression.Method == _isLikeMethod)
				{
					convertedExpression = IsLikeCall<TEntity>(methodCallExpression, expressionConverter);
					return true;
				}
				else if (_minMethods.Contains(methodCallExpression.Method))
				{
					convertedExpression = MinCall<TEntity>(methodCallExpression, expressionConverter);
					return true;
				}
				else if (_maxMethods.Contains(methodCallExpression.Method))
				{
					convertedExpression = MaxCall<TEntity>(methodCallExpression, expressionConverter);
					return true;
				}
				else if(_countMethods.Contains(methodCallExpression.Method))
				{
					convertedExpression = CountCall<TEntity>(methodCallExpression, expressionConverter);
					return true;
				}
			}

			convertedExpression = default;
			return false;
		}

		private LinqQueryExpression<TEntity> CountCall<TEntity>(MethodCallExpression methodCallExpression,
			ExpressionConversionVisitor<TEntity> expressionConverter)
			where TEntity : class
		{
			if (methodCallExpression.Arguments.Count < 2)
				return new LinqQueryExpression<TEntity>(
					QueryExpression.CountFunction()
				);

			var expression = expressionConverter.Visit(methodCallExpression.Arguments[1]) as LinqQueryExpression<TEntity>;
			return new LinqQueryExpression<TEntity>(
				QueryExpression.CountFunction(expression.QueryExpression),
				expression.JoinBuilders
				);
		}

		private LinqQueryExpression<TEntity> IsLikeCall<TEntity>(MethodCallExpression methodCallExpression,
			ExpressionConversionVisitor<TEntity> expressionConverter)
			where TEntity : class
		{
			var expression = expressionConverter.Visit(methodCallExpression.Arguments[1]) as LinqQueryExpression<TEntity>;
			var value = expressionConverter.Visit(methodCallExpression.Arguments[2]) as LinqQueryExpression<TEntity>;
			return new LinqQueryExpression<TEntity>(
				QueryExpression.Compare(
					expression.QueryExpression,
					ComparisonOperator.Like,
					value.QueryExpression
					),
				expression.JoinBuilders
				);
		}

		private LinqQueryExpression<TEntity> IsInCall<TEntity>(MethodCallExpression methodCallExpression,
			ExpressionConversionVisitor<TEntity> expressionConverter)
			where TEntity : class
		{
			var expression = expressionConverter.Visit(methodCallExpression.Arguments[1]) as LinqQueryExpression<TEntity>;
			var value = expressionConverter.Visit(methodCallExpression.Arguments[2]) as LinqQueryExpression<TEntity>;

			if (value.QueryExpression is ValueExpression valueQueryExpression &&
				valueQueryExpression.Value is IEnumerable valueEnumerable)
			{
				var valueExpressions = new List<QueryExpression>();
				foreach (var o in valueEnumerable)
				{
					valueExpressions.Add(ORMQueryExpressions.Value(o));
				}

				return new LinqQueryExpression<TEntity>(
					QueryExpression.Compare(
						expression.QueryExpression,
						ComparisonOperator.None,
						QueryExpression.InFunction(valueExpressions.ToArray())
						),
						expression.JoinBuilders);
			}

			return new LinqQueryExpression<TEntity>(
				QueryExpression.Compare(
					expression.QueryExpression,
					ComparisonOperator.None,
					QueryExpression.InFunction(value.QueryExpression)
					),
					expression.JoinBuilders);
		}

		private LinqQueryExpression<TEntity> HasFlagCall<TEntity>(MethodCallExpression methodCallExpression,
			ExpressionConversionVisitor<TEntity> expressionConverter)
			where TEntity : class
		{
			var expression = expressionConverter.Visit(methodCallExpression.Arguments[1]) as LinqQueryExpression<TEntity>;
			var value = expressionConverter.Visit(methodCallExpression.Arguments[2]) as LinqQueryExpression<TEntity>;

			return new LinqQueryExpression<TEntity>(
				QueryExpression.Compare(
					new BitwiseOperationQueryExpression(expression.QueryExpression, BitwiseOperator.And, value.QueryExpression),
					ComparisonOperator.AreEqual,
					value.QueryExpression
					),
					(expression.JoinBuilders ?? JoinBuilder<TEntity>.Empty).Concat(
						value.JoinBuilders ?? JoinBuilder<TEntity>.Empty
						));
		}

		private LinqQueryExpression<TEntity> RandomCall<TEntity>()
			where TEntity : class
		{
			return new LinqQueryExpression<TEntity>(
				QueryExpression.Random()
				);
		}

		private LinqQueryExpression<TEntity> LastInsertIdCall<TEntity>()
			where TEntity : class
		{
			return new LinqQueryExpression<TEntity>(
				QueryExpression.LastInsertIdFunction()
				);
		}

		private LinqQueryExpression<TEntity> MinCall<TEntity>(MethodCallExpression methodCallExpression,
			ExpressionConversionVisitor<TEntity> expressionConverter)
			where TEntity : class
		{
			var expression = expressionConverter.Visit(methodCallExpression.Arguments[1]) as LinqQueryExpression<TEntity>;

			return new LinqQueryExpression<TEntity>(
				QueryExpression.Min(expression.QueryExpression),
				expression.JoinBuilders
				);
		}

		private LinqQueryExpression<TEntity> MaxCall<TEntity>(MethodCallExpression methodCallExpression,
			ExpressionConversionVisitor<TEntity> expressionConverter)
			where TEntity : class
		{
			var expression = expressionConverter.Visit(methodCallExpression.Arguments[1]) as LinqQueryExpression<TEntity>;

			return new LinqQueryExpression<TEntity>(
				QueryExpression.Max(expression.QueryExpression),
				expression.JoinBuilders
				);
		}
	}
}
