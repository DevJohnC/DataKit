namespace DataKit.SQL.QueryExpressions
{
	public enum ExpressionType
	{
		/// <summary>
		/// An SQL statement expression: SELECT, UPDATE, INSERT, DELETE, etc.
		/// </summary>
		Statement,
		/// <summary>
		/// An SQL function call on the SQL server.
		/// </summary>
		FunctionCall,
		/// <summary>
		/// A series of expressions being projected.
		/// </summary>
		Projection,
		/// <summary>
		/// A query parameter such as FROM, WHERE, ORDER BY etc.
		/// </summary>
		QueryParameter,
		/// <summary>
		/// Identifies a named item. ie, a column, an aliased projection, a table, etc.
		/// </summary>
		Identifier,
		/// <summary>
		/// An extension expression.
		/// </summary>
		Extension = int.MaxValue
	}
}
