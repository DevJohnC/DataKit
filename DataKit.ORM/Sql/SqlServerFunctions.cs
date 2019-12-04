using DataKit.ORM.Sql;
using System;
using System.Collections.Generic;

namespace DataKit.ORM
{
	public static class SqlServerFunctionsExtensions
	{
		public static int LastInsertId(this SqlServerFunctions s) => 0;

		public static int Random(this SqlServerFunctions s) => 0;

		public static int Count(this SqlServerFunctions s)
			=> 0;

		public static int Count(this SqlServerFunctions s, object expression)
			=> 0;

		public static bool IsLike(this SqlServerFunctions s, string str, string pattern)
			=> false;

		public static bool IsIn<TProperty>(this SqlServerFunctions s, TProperty value, IEnumerable<TProperty> collection)
			=> false;

		public static bool IsIn<TProperty>(this SqlServerFunctions s, TProperty value, IProjectionQuery<TProperty> valuesQuery)
			=> false;

		public static bool HasFlag<TProperty>(this SqlServerFunctions s, TProperty value, TProperty flag)
			where TProperty : Enum
			=> false;

		public static byte Min(this SqlServerFunctions s, byte expr) => 0;
		public static short Min(this SqlServerFunctions s, short expr) => 0;
		public static int Min(this SqlServerFunctions s, int expr) => 0;
		public static long Min(this SqlServerFunctions s, long expr) => 0;
		public static float Min(this SqlServerFunctions s, float expr) => 0f;
		public static double Min(this SqlServerFunctions s, double expr) => 0d;
		public static decimal Min(this SqlServerFunctions s, decimal expr) => 0m;

		public static byte Max(this SqlServerFunctions s, byte expr) => 0;
		public static short Max(this SqlServerFunctions s, short expr) => 0;
		public static int Max(this SqlServerFunctions s, int expr) => 0;
		public static long Max(this SqlServerFunctions s, long expr) => 0;
		public static float Max(this SqlServerFunctions s, float expr) => 0f;
		public static double Max(this SqlServerFunctions s, double expr) => 0d;
		public static decimal Max(this SqlServerFunctions s, decimal expr) => 0m;
	}
}
