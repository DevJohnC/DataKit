using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DataKit.Mapping
{
	internal static class BoxingHelper
	{
		private static object BoxIfNeededImpl<T>(T value) => value;

		private static T UnboxIfNeededImpl<T>(object value) => (T)value;

		private static MethodInfo _boxIfNeeded = typeof(BoxingHelper)
			.GetMethod(nameof(BoxIfNeededImpl), BindingFlags.NonPublic | BindingFlags.Static);

		private static MethodInfo _unboxIfNeeded = typeof(BoxingHelper)
			.GetMethod(nameof(UnboxIfNeededImpl), BindingFlags.NonPublic | BindingFlags.Static);

		public static Expression UnboxIfNeeded(Type valueType, Expression valueExpression)
		{
			return Expression.Call(
				_unboxIfNeeded.MakeGenericMethod(valueType),
				valueExpression
				);
		}

		public static Expression BoxIfNeeded(Type valueType, Expression valueExpression)
		{
			return Expression.Call(
				_boxIfNeeded.MakeGenericMethod(valueType),
				valueExpression
				);
		}
	}
}
