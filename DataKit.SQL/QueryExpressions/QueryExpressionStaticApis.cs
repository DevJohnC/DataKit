using System.Collections.Generic;
using System.Linq;

namespace DataKit.SQL.QueryExpressions
{
	public partial class QueryExpression
	{
		public static SelectStatementQueryExpression SelectStatement(
			QueryExpression[] projection,
			QueryExpression from = null,
			QueryExpression where = null,
			QueryExpression having = null
			)
		{
			return new SelectStatementQueryExpression(
				new ProjectionQueryExpression(projection),
				from != null ? new FromQueryExpression(from) : null,
				where != null ? new WhereQueryExpression(where) : null,
				having != null ? new HavingQueryExpression(having) : null
				);
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
			return new IsInOperatorQueryExpression(expression, inExpressions);
		}

		public static ColumnIdentifierQueryExpression Column(string columnName, IdentifierQueryExpression parent = null)
		{
			return new ColumnIdentifierQueryExpression(columnName, parent);
		}

		public static TableIdentifierQueryExpression Table(string tableName)
		{
			return new TableIdentifierQueryExpression(tableName);
		}

		public static CountFunctionCallQueryExpression CountFunction(
			QueryExpression expression = null
			)
		{
			return new CountFunctionCallQueryExpression(expression);
		}
	}
}
