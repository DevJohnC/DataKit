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
