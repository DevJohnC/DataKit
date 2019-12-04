using DataKit.ORM.Schema;
using DataKit.ORM.Schema.Sql;
using DataKit.ORM.Sql.QueryBuilding;
using Silk.Data.SQL.Expressions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DataKit.ORM.Sql.Expressions
{
	public class ExpressionConversionVisitor<TEntity> : ExpressionVisitor
		where TEntity : class
	{
		public ExpressionConversionVisitor(SqlDataModel<TEntity> dataModel, IAliasIdentifier tableIdentifier = null)
		{
			DataModel = dataModel;
			_dataSchema = dataModel.DataSchema;
			_tableIdentifier = tableIdentifier ?? new ImmutableIdentifier(dataModel.StorageModel.DefaultTableName);
		}

		public IAliasIdentifier TableIdentifier => _tableIdentifier;

		public SqlDataModel<TEntity> DataModel { get; }

		private readonly DataSchema _dataSchema;
		private readonly IAliasIdentifier _tableIdentifier;
		private ReadOnlyCollection<ParameterExpression> _parameters;

		public Expression VisitMain<T>(Expression<T> node)
		{
			_parameters = node.Parameters;
			return Visit(node);
		}

		private QueryExpression GetValueExpression(object value)
		{
			if (value is QueryExpression queryExpression)
				return queryExpression;

			if (value is SqlQueryBuilder queryBuilder)
				return queryBuilder.BuildQuery();

			if (value is IProjectionQuery projectionQuery)
				return projectionQuery.BuildQuery();

			return ORMQueryExpressions.Value(value);
		}

		private IEnumerable<Expression> FlattenMemberExpressionTree(MemberExpression node)
		{
			return CrawlTree(node);

			IEnumerable<Expression> CrawlTree(Expression expression)
			{
				if (expression is MemberExpression memberExpression && memberExpression?.Expression != null)
				{
					foreach (var childNode in CrawlTree(memberExpression.Expression))
					{
						yield return childNode;
					}
				}

				yield return expression;
			}
		}

		private bool IsEntityParameter(Expression expression)
		{
			return _parameters.Any(q => q == expression);
		}

		protected override Expression VisitConstant(ConstantExpression node)
		{
			//  unchanging value
			return new LinqQueryExpression<TEntity>(GetValueExpression(node.Value));
		}

		private Expression ConvertEntityMemberToColumn(Expression[] expressionTree)
		{
			//  compare the member's path on the entity to the path the storage fields were derived from
			var modelPath = expressionTree.Skip(1)
				.OfType<MemberExpression>()
				.Select(q => q.Member.Name);
			var storageField = DataModel.StorageModel.Fields.FirstOrDefault(q => q.TypeModelGraphPath.Path.SequenceEqual(modelPath));

			if (storageField == null)
				throw new Exception("Couldn't resolve storage field.");

			if (!storageField.RequiresJoin)
				return new LinqQueryExpression<TEntity>(
					new ColumnWithAliasSourceQueryExpression(storageField.ColumnName, _tableIdentifier, storageField)
					);

			var joins = storageField.JoinSpecification.CreateJoin(_dataSchema, storageField, _tableIdentifier)
				.ToArray();
			return new LinqQueryExpression<TEntity>(
				new ColumnWithAliasSourceQueryExpression(storageField.ColumnName, joins.Last().Alias, storageField),
				joins.Select(q => q.Builder).ToArray()
				);
		}

		protected override Expression VisitMember(MemberExpression node)
		{
			//  node.Expression is the Expression being operated on
			//  node.Member is the member ON node.Expression being accessed
			var flatExpressionTree = FlattenMemberExpressionTree(node)
				.ToArray();

			if (IsEntityParameter(flatExpressionTree[0]))
			{
				return ConvertEntityMemberToColumn(flatExpressionTree);
			}
			else
			{
				//  todo: instead of compiling the entire expression we should navigate the expression tree to read the value at the end
				var memberAccessExp = Expression.MakeMemberAccess(node.Expression, node.Member);
				var @delegate = Expression.Lambda<Func<object>>(
					Expression.Convert(memberAccessExp, typeof(object))
					);
				var value = @delegate.Compile()();

				return new LinqQueryExpression<TEntity>(GetValueExpression(value));
			}
		}

		private bool IsServerMethod(MethodInfo method)
		{
			if (!method.IsStatic || method.GetCustomAttribute<ExtensionAttribute>() == null)
				return false;
			var parameters = method.GetParameters();
			if (parameters.Length < 1 || parameters[0].ParameterType != typeof(SqlServerFunctions))
				return false;
			return true;
		}

		private bool MethodUsesEntityInstance(MethodCallExpression methodExpression)
		{
			//  todo: check if an instance of a db entity is required to run the method
			return false;
		}

		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if (IsServerMethod(node.Method))
			{
				foreach (var methodCallConverter in _dataSchema.Sql.SqlMethodConverters)
				{
					if (methodCallConverter.TryConvertMethod(node, this, out var expression))
					{
						return expression;
					}
				}
				throw new InvalidOperationException("Failed to convert SQL function to SQL expression.");
			}
			else if (!MethodUsesEntityInstance(node))
			{
				//  run local code
			}
			throw new InvalidOperationException("Unable to execute non-server method that requires an entity instance.");
		}

		protected override Expression VisitBinary(BinaryExpression node)
		{
			var left = Visit(node.Left) as LinqQueryExpression<TEntity>;
			if (left == null)
				throw new InvalidOperationException("Couldn't convert expression to SQL query expression.");

			var right = Visit(node.Right) as LinqQueryExpression<TEntity>;
			if (right == null)
				throw new InvalidOperationException("Couldn't convert expression to SQL query expression.");

			var leftExpression = left.QueryExpression;
			var rightExpression = right.QueryExpression;

			switch (node.NodeType)
			{
				case ExpressionType.AddChecked:
				case ExpressionType.Add:
					return new LinqQueryExpression<TEntity>(
						QueryExpression.Add(leftExpression, rightExpression)
						);
				case ExpressionType.SubtractChecked:
				case ExpressionType.Subtract:
					return new LinqQueryExpression<TEntity>(
						QueryExpression.Subtract(leftExpression, rightExpression)
						);
				case ExpressionType.MultiplyChecked:
				case ExpressionType.Multiply:
					return new LinqQueryExpression<TEntity>(
						QueryExpression.Multiply(leftExpression, rightExpression)
						);
				case ExpressionType.Divide:
					return new LinqQueryExpression<TEntity>(
						QueryExpression.Divide(leftExpression, rightExpression)
						);

				case ExpressionType.And:
					return new LinqQueryExpression<TEntity>(
						new BitwiseOperationQueryExpression(leftExpression, BitwiseOperator.And, rightExpression)
						);
				case ExpressionType.ExclusiveOr:
					return new LinqQueryExpression<TEntity>(
						new BitwiseOperationQueryExpression(leftExpression, BitwiseOperator.ExclusiveOr, rightExpression)
						);
				case ExpressionType.Or:
					return new LinqQueryExpression<TEntity>(
						new BitwiseOperationQueryExpression(leftExpression, BitwiseOperator.Or, rightExpression)
						);

				case ExpressionType.AndAlso:
					return new LinqQueryExpression<TEntity>(
						QueryExpression.AndAlso(leftExpression, rightExpression)
						);
				case ExpressionType.OrElse:
					return new LinqQueryExpression<TEntity>(
						QueryExpression.OrElse(leftExpression, rightExpression)
						);

				case ExpressionType.Equal:
					return new LinqQueryExpression<TEntity>(
						QueryExpression.Compare(leftExpression, ComparisonOperator.AreEqual, rightExpression)
						);
				case ExpressionType.NotEqual:
					return new LinqQueryExpression<TEntity>(
						QueryExpression.Compare(leftExpression, ComparisonOperator.AreNotEqual, rightExpression)
						);
				case ExpressionType.GreaterThan:
					return new LinqQueryExpression<TEntity>(
						QueryExpression.Compare(leftExpression, ComparisonOperator.GreaterThan, rightExpression)
						);
				case ExpressionType.GreaterThanOrEqual:
					return new LinqQueryExpression<TEntity>(
						QueryExpression.Compare(leftExpression, ComparisonOperator.GreaterThanOrEqualTo, rightExpression)
						);
				case ExpressionType.LessThan:
					return new LinqQueryExpression<TEntity>(
						QueryExpression.Compare(leftExpression, ComparisonOperator.LessThan, rightExpression)
						);
				case ExpressionType.LessThanOrEqual:
					return new LinqQueryExpression<TEntity>(
						QueryExpression.Compare(leftExpression, ComparisonOperator.LessThanOrEqualTo, rightExpression)
						);

				default:
					throw new Exception($"Unsupported binary node type '{node.NodeType}'.");
			}
		}

		protected override Expression VisitParameter(ParameterExpression node)
		{
			//  visiting a parameter on the lambda that was first passed into Visit to start
			//  the conversion.
			//  Since other VisitX methods short-circuit the node graph being visited
			//  this method should only be executed when the expression is of the form:
			//    o => o
			//  therefore we just return a table expression
			return new LinqQueryExpression<TEntity>(
				new AliasReferenceQueryExpression(_tableIdentifier)
				);
		}

		protected override Expression VisitLambda<T>(Expression<T> node)
		{
			//  VisitMain should end up here as will Visit(..) when being called
			//  to convert server method arguments that are expressions
			//  swap out the parameters args for the lifetime of the conversion
			//  another approach in the future would be to force the caller
			//  to build a whole new ExpressionConversionVisitor
			var swap = _parameters;
			_parameters = node.Parameters;
			try
			{
				return Visit(node.Body);
			}
			finally
			{
				_parameters = swap;
			}
		}

		protected override Expression VisitUnary(UnaryExpression node)
		{
			//  expression arguments for server method calls are often wrapped in a unary node
			//  ie: Count(q => q.Id)
			//  this often makes the base implementation angry about our custom expressions so we ignore all that
			return Visit(node.Operand);
		}

		protected override Expression VisitNewArray(NewArrayExpression node)
		{
			//  expression arguments that use arrays directly
			//  like: IsIn(q => .., new[] { 1, 2, 3, 4 })
			//  return a value expression with the desired array inside
			var @delegate = Expression.Lambda<Func<object>>(
				Expression.Convert(node, typeof(object))
				);
			var value = @delegate.Compile()();

			return new LinqQueryExpression<TEntity>(GetValueExpression(value));
		}

		private class ImmutableIdentifier : IAliasIdentifier
		{
			public string AliasIdentifier { get; }

			public ImmutableIdentifier(string aliasIdentifier)
			{
				AliasIdentifier = aliasIdentifier;
			}
		}
	}
}
