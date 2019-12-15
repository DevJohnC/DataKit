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

		public static CountFunctionCallQueryExpression CountFunction(
			QueryExpression expression = null
			)
		{
			return new CountFunctionCallQueryExpression(expression);
		}
	}
}
