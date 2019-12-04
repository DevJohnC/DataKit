using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DataKit.ORM
{
	internal static class InternalTypeExtensions
	{
		private static readonly Type _sqlDataSetEntityType = typeof(SqlDataSet<>);
		private static readonly Type _sqlDataSetEntityBusinessType = typeof(SqlDataSet<,>);

		public static IEnumerable<PropertyInfo> GetDataSetProperties(this Type type)
		{
			return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
					.Where(q => q.CanRead && !q.GetMethod.IsStatic && q.CanWrite && !q.SetMethod.IsStatic &&
							q.PropertyType.IsSubclassOf(typeof(DataSet)));
		}

		public static bool IsSqlDataSetWithEntityAndBusinessTypes(this Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == _sqlDataSetEntityBusinessType;
		}

		public static bool IsSqlDataSetWithEntityType(this Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == _sqlDataSetEntityType;
		}
	}
}
