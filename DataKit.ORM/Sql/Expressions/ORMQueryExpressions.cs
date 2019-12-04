using Silk.Data.SQL.Expressions;
using System;

namespace DataKit.ORM.Sql.Expressions
{
	public static class ORMQueryExpressions
	{
		public static ValueExpression Value(object value)
		{
			if (value is Enum)
				value = (int)value;
			return QueryExpression.Value(value);
		}
	}
}
