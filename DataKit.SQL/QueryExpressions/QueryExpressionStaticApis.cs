using System.Collections.Generic;
using System.Linq;

namespace DataKit.SQL.QueryExpressions
{
	public partial class QueryExpression
	{
		public static SelectStatementQueryExpression Select(
			QueryExpression[] projection,
			QueryExpression from = null,
			JoinQueryExpression[] joins = null,
			QueryExpression where = null,
			QueryExpression having = null,
			OrderByQueryExpression[] orderBy = null,
			GroupByQueryExpression[] groupBy = null,
			LimitQueryExpression limit = null
			)
		{
			return new SelectStatementQueryExpression(
				new ProjectionQueryExpression(projection),
				from != null ? new FromQueryExpression(from) : null,
				joins,
				where != null ? new WhereQueryExpression(where) : null,
				having != null ? new HavingQueryExpression(having) : null,
				orderBy, groupBy, limit
				);
		}

		public static JoinQueryExpression Join(QueryExpression targetExpression, ColumnIdentifierQueryExpression leftColumn, ColumnIdentifierQueryExpression rightColumn,
			JoinDirection direction = JoinDirection.Inner)
		{
			return new JoinQueryExpression(targetExpression, direction,
				AreEqual(leftColumn, rightColumn));
		}

		public static JoinQueryExpression Join(QueryExpression targetExpression, QueryExpression onExpression,
			JoinDirection direction = JoinDirection.Inner)
		{
			return new JoinQueryExpression(targetExpression, direction, onExpression);
		}

		public static ParameterReferenceQueryExpression Parameter(string parameterName)
		{
			return new ParameterReferenceQueryExpression(parameterName);
		}

		public static ValueParameterQueryExpression Value(object value)
		{
			return new ValueParameterQueryExpression(value);
		}

		public static AsOperatorQueryExpression As(QueryExpression expression, AliasIdentifierQueryExpression alias)
		{
			return new AsOperatorQueryExpression(expression, alias);
		}

		public static AsOperatorQueryExpression As(QueryExpression expression, string alias, out AliasIdentifierQueryExpression aliasIdentifierQueryExpression)
		{
			aliasIdentifierQueryExpression = new AliasIdentifierQueryExpression(alias);
			return new AsOperatorQueryExpression(expression, aliasIdentifierQueryExpression);
		}

		public static IsInOperatorQueryExpression IsIn(QueryExpression expression, SelectStatementQueryExpression selectQuery)
		{
			return IsIn(expression, new[] { selectQuery });
		}

		public static IsInOperatorQueryExpression IsIn(QueryExpression expression, IEnumerable<QueryExpression> inExpressions)
		{
			return IsIn(expression, inExpressions.ToArray());
		}

		public static IsInOperatorQueryExpression IsIn(QueryExpression expression, params QueryExpression[] inExpressions)
		{
			return new IsInOperatorQueryExpression(expression, inExpressions, false);
		}

		public static IsInOperatorQueryExpression IsNotIn(QueryExpression expression, SelectStatementQueryExpression selectQuery)
		{
			return IsNotIn(expression, new[] { selectQuery });
		}

		public static IsInOperatorQueryExpression IsNotIn(QueryExpression expression, IEnumerable<QueryExpression> inExpressions)
		{
			return IsNotIn(expression, inExpressions.ToArray());
		}

		public static IsInOperatorQueryExpression IsNotIn(QueryExpression expression, params QueryExpression[] inExpressions)
		{
			return new IsInOperatorQueryExpression(expression, inExpressions, true);
		}

		public static UnaryOperatorQueryExpression IsNull(QueryExpression expression)
		{
			return new UnaryOperatorQueryExpression(expression, UnaryOperator.IsNull);
		}

		public static UnaryOperatorQueryExpression IsNotNull(QueryExpression expression)
		{
			return new UnaryOperatorQueryExpression(expression, UnaryOperator.IsNotNull);
		}

		public static UnaryOperatorQueryExpression Distinct(QueryExpression expression)
		{
			return new UnaryOperatorQueryExpression(expression, UnaryOperator.Distinct);
		}

		public static ColumnIdentifierQueryExpression All(IdentifierQueryExpression parent = null)
		{
			return Column("*", parent);
		}

		public static BinaryOperatorQueryExpression AndAlso(QueryExpression left, QueryExpression right)
		{
			return new BinaryOperatorQueryExpression(left, right, BinaryOperator.AndAlso);
		}

		public static BinaryOperatorQueryExpression OrElse(QueryExpression left, QueryExpression right)
		{
			return new BinaryOperatorQueryExpression(left, right, BinaryOperator.OrElse);
		}

		public static BinaryOperatorQueryExpression And(QueryExpression left, QueryExpression right)
		{
			return new BinaryOperatorQueryExpression(left, right, BinaryOperator.BitwiseAnd);
		}

		public static BinaryOperatorQueryExpression Or(QueryExpression left, QueryExpression right)
		{
			return new BinaryOperatorQueryExpression(left, right, BinaryOperator.BitwiseOr);
		}

		public static BinaryOperatorQueryExpression ExclusiveOr(QueryExpression left, QueryExpression right)
		{
			return new BinaryOperatorQueryExpression(left, right, BinaryOperator.BitwiseExclusiveOr);
		}

		public static BinaryOperatorQueryExpression Add(QueryExpression left, QueryExpression right)
		{
			return new BinaryOperatorQueryExpression(left, right, BinaryOperator.Addition);
		}

		public static BinaryOperatorQueryExpression Subtract(QueryExpression left, QueryExpression right)
		{
			return new BinaryOperatorQueryExpression(left, right, BinaryOperator.Subtraction);
		}

		public static BinaryOperatorQueryExpression Divide(QueryExpression left, QueryExpression right)
		{
			return new BinaryOperatorQueryExpression(left, right, BinaryOperator.Division);
		}

		public static BinaryOperatorQueryExpression Multiply(QueryExpression left, QueryExpression right)
		{
			return new BinaryOperatorQueryExpression(left, right, BinaryOperator.Multiplication);
		}

		public static BinaryOperatorQueryExpression Modulus(QueryExpression left, QueryExpression right)
		{
			return new BinaryOperatorQueryExpression(left, right, BinaryOperator.Modulus);
		}

		public static BinaryOperatorQueryExpression AreEqual(QueryExpression left, QueryExpression right)
		{
			return new BinaryOperatorQueryExpression(left, right, BinaryOperator.AreEqual);
		}

		public static BinaryOperatorQueryExpression AreNotEqual(QueryExpression left, QueryExpression right)
		{
			return new BinaryOperatorQueryExpression(left, right, BinaryOperator.AreNotEqual);
		}

		public static BinaryOperatorQueryExpression GreaterThan(QueryExpression left, QueryExpression right)
		{
			return new BinaryOperatorQueryExpression(left, right, BinaryOperator.GreaterThan);
		}

		public static BinaryOperatorQueryExpression GreaterThanOrEqualTo(QueryExpression left, QueryExpression right)
		{
			return new BinaryOperatorQueryExpression(left, right, BinaryOperator.GreaterThanOrEqualTo);
		}

		public static BinaryOperatorQueryExpression LessThan(QueryExpression left, QueryExpression right)
		{
			return new BinaryOperatorQueryExpression(left, right, BinaryOperator.LessThan);
		}

		public static BinaryOperatorQueryExpression LessThanOrEqualTo(QueryExpression left, QueryExpression right)
		{
			return new BinaryOperatorQueryExpression(left, right, BinaryOperator.LessThanOrEqualTo);
		}

		public static BinaryOperatorQueryExpression Like(QueryExpression left, QueryExpression right)
		{
			return new BinaryOperatorQueryExpression(left, right, BinaryOperator.Like);
		}

		public static BinaryOperatorQueryExpression NotLike(QueryExpression left, QueryExpression right)
		{
			return new BinaryOperatorQueryExpression(left, right, BinaryOperator.NotLike);
		}

		public static ColumnIdentifierQueryExpression Column(string columnName, IdentifierQueryExpression parent = null)
		{
			return new ColumnIdentifierQueryExpression(columnName, parent);
		}

		public static TableIdentifierQueryExpression Table(string tableName)
		{
			return new TableIdentifierQueryExpression(tableName);
		}

		public static OrderByQueryExpression OrderBy(QueryExpression expression)
		{
			return new OrderByQueryExpression(expression, OrderByDirection.Ascending);
		}

		public static OrderByQueryExpression OrderByDescending(QueryExpression expression)
		{
			return new OrderByQueryExpression(expression, OrderByDirection.Descending);
		}

		public static GroupByQueryExpression GroupBy(QueryExpression expression)
		{
			return new GroupByQueryExpression(expression);
		}

		public static LimitQueryExpression Limit(QueryExpression limit)
		{
			return new LimitQueryExpression(limit);
		}

		public static LimitQueryExpression Limit(QueryExpression limit, QueryExpression offset)
		{
			return new LimitQueryExpression(limit, offset);
		}

		public static CountFunctionCallQueryExpression CountFunction(
			QueryExpression expression = null
			)
		{
			return new CountFunctionCallQueryExpression(expression);
		}
	}
}
