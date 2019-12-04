using DataKit.Modelling.TypeModels;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DataKit.Mapping.Binding
{
	public class ReadFieldTransform
	{
		public static BindingTransformation Create(params PropertyField[] fields)
			=> Create((IReadOnlyList<PropertyField>)fields);

		public static BindingTransformation Create(IReadOnlyList<PropertyField> fields)
		{
			var context = Expression.Parameter(typeof(BindingContext));
			var inObj = Expression.Parameter(typeof(object), "in");
			var outObj = Expression.Parameter(typeof(object).MakeByRefType(), "out");

			var assignment = Expression.Assign(
				outObj,
				BoxingHelper.BoxIfNeeded(fields.Last().FieldType.Type, GetPropertyExpression(inObj, fields))
				);
			var result = Expression.Variable(typeof(bool), "result");
			var body = Expression.Block(
				new[] { result },
				Expression.IfThenElse(
					CreateNullCheck(inObj, fields),
					Expression.Block(assignment, Expression.Assign(result, Expression.Constant(true))),
					Expression.Assign(result, Expression.Constant(false))
				),
				result);

			return Expression.Lambda<BindingTransformation>(
				body, context, inObj, outObj
				).Compile();
		}

		private static Expression CreateNullCheck(ParameterExpression root, IReadOnlyList<PropertyField> fields)
		{
			if (fields.Count == 1)
				return Expression.Constant(true);

			var nullCheckPassed = Expression.Variable(typeof(bool));
			var nullCheckExpr = CreateNullCheck(root, nullCheckPassed, fields, 0);
			return Expression.Block(new[] { nullCheckPassed }, nullCheckExpr, nullCheckPassed);
		}

		private static Expression CreateNullCheck(ParameterExpression root, ParameterExpression nullCheckPassed, IReadOnlyList<PropertyField> fields, int offset)
		{
			if (offset == fields.Count - 1)
				return Expression.Assign(nullCheckPassed, Expression.Constant(true));

			return Expression.IfThenElse(
				Expression.Equal(GetPropertyExpression(root, fields.Take(offset + 1)), Expression.Constant(null)),
				Expression.Assign(nullCheckPassed, Expression.Constant(false)),
				CreateNullCheck(root, nullCheckPassed, fields, offset + 1)
				);
		}

		private static Expression GetPropertyExpression(Expression root, IEnumerable<PropertyField> fields)
		{
			Expression expr = Expression.Convert(root, fields.First().Property.DeclaringType);
			foreach(var field in fields)
			{
				expr = Expression.Property(expr, field.Property);
			}
			return expr;
		}
	}
}
