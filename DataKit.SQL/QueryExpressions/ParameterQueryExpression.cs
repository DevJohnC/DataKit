using System;
using System.Collections.Generic;
using System.Text;

namespace DataKit.SQL.QueryExpressions
{
	public abstract class ParameterQueryExpression : QueryExpression
	{
		public override ExpressionType ExpressionType => ExpressionType.Parameter;

		protected override QueryExpression Accept(QueryExpressionVisitor expressionVisitor)
			=> expressionVisitor.VisitParameter(this);
	}

	public class ParameterReferenceQueryExpression : ParameterQueryExpression
	{
		public ParameterReferenceQueryExpression(string parameterName)
		{
			ParameterName = NormalizeParameterName(parameterName) ?? throw new ArgumentNullException(nameof(parameterName));
		}

		public string ParameterName { get; }

		private static string NormalizeParameterName(string parameterName)
		{
			if (parameterName == null)
				return null;

			while (parameterName.StartsWith("@"))
				parameterName = parameterName.Substring(1);
			return parameterName;
		}
	}

	public class ValueParameterQueryExpression : ParameterQueryExpression
	{
		public ValueParameterQueryExpression(object value)
		{
			Value = value;
		}

		public new object Value { get; }
	}
}
