using DataKit.SQL.QueryExpressions;
using System;

namespace DataKit.ORM.Sql.Expressions
{
	public static class ORMQueryExpressions
	{
		public static ValueParameterQueryExpression Value(object value)
		{
			if (value is Enum)
				value = (int)value;
			return QueryExpression.Value(value);
		}
	}
}
