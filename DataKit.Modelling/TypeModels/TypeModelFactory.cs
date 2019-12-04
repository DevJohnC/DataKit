using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DataKit.Modelling.TypeModels
{
	internal static class TypeModelFactory
	{
		/// <summary>
		/// Cached reference to the generic build model method.
		/// </summary>
		private static readonly MethodInfo _createModelMethod = typeof(TypeModelFactory)
			.GetMethod(nameof(BuildTypeModelGeneric), BindingFlags.Static | BindingFlags.NonPublic);

		/// <summary>
		/// Build the TypeModel for the provided Type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static TypeModel BuildTypeModel(Type type)
			=> _createModelMethod.MakeGenericMethod(type).Invoke(null, null) as TypeModel;

		/// <summary>
		/// Build a TypeModel for the provided generic type parameter.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		private static TypeModel<T> BuildTypeModelGeneric<T>()
		{
			var fields = GetPropertyFields(typeof(T));
			return new TypeModel<T>(fields);
		}

		/// <summary>
		/// Gets all eligable fields for the type model of the provided Type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		private static IReadOnlyList<PropertyField> GetPropertyFields(Type type)
		{
			var ret = new List<PropertyField>();
			foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
					.Where(q => (q.CanRead && !q.GetMethod.IsStatic) ||
						(q.CanWrite && !q.SetMethod.IsStatic)))
			{
				ret.Add(PropertyField.CreateFromPropertyInfo(property));
			}
			return ret;
		}
	}
}
