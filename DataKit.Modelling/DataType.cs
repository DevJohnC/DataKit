using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DataKit.Modelling
{
	/// <summary>
	/// Describes a data type.
	/// </summary>
	public abstract class DataType
	{
		/// <summary>
		/// Gets the data type.
		/// </summary>
		public abstract Type Type { get; }

		public abstract bool IsEnumerable { get; }

		public abstract Type ElementType { get; }

		/// <summary>
		/// Gets a value indicating if the enumerable type is pure.
		/// </summary>
		/// <remarks>
		/// This is helpful for catching types like <see cref="String">string</see> which are technically an enumerable type but are more useful as their usual counterpart.
		/// </remarks>
		public abstract bool IsPureEnumerable { get; }

		/// <summary>
		/// Gets a value indicating if the type is a compiler generated anonymous type.
		/// </summary>
		public abstract bool IsAnonymousType { get; }

		private static readonly CachedCollection<Type, Type, DataType> _instanceCache
			= new CachedCollection<Type, Type, DataType>(CreateNew);

		public static DataType GetTypeOf(Type type)
		{
			return _instanceCache.GetOrCreate(type, type);
		}

		public static DataType<T> GetTypeOf<T>()
			=> GetTypeOf(typeof(T)) as DataType<T>;

		private static DataType CreateNew(Type type)
		{
			var ctor = typeof(DataType<>).MakeGenericType(type)
				.GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				.First();
			return ctor.Invoke(null) as DataType;
		}
	}

	/// <summary>
	/// Describes a data type of type T.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class DataType<T> : DataType
	{
		public override Type Type => typeof(T);

		public override bool IsEnumerable { get; }

		public override Type ElementType { get; }

		public override bool IsPureEnumerable { get; }

		public override bool IsAnonymousType { get; }

		private DataType()
		{
			ElementType = GetEnumerableElementType();
			IsEnumerable = ElementType != null;
			IsPureEnumerable = IsEnumerable && IsPureEnumerableType(ElementType);
			IsAnonymousType = DetectIsAnonymousType();
		}

		private static Type _enumerableCompareType = typeof(IEnumerable<>);

		private static bool DetectIsAnonymousType()
		{
			var checkType = typeof(T);
			if (checkType.IsClass && checkType.IsSealed && checkType.IsNotPublic && checkType.BaseType == typeof(object)
				&& checkType.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false) != null)
				return true;
			return false;
		}

		private static bool IsPureEnumerableType(Type elementType)
		{
			var checkType = typeof(T);
			var collectionTypes = new[]
			{
				typeof(ICollection),
				typeof(ICollection<>).MakeGenericType(elementType),
				typeof(IReadOnlyCollection<>).MakeGenericType(elementType)
			};

			//  checkType is not an implementation of IEnumerable but IEnumerable/ICollection directly, it's pure
			if (checkType == typeof(IEnumerable) || collectionTypes.Contains(checkType))
				return true;

			//  checkType is an array, it's pure
			if (checkType.IsArray)
				return true;

			var checkInterfaces = checkType.GetInterfaces();

			//  collection types are considered pure, this takes care of just about all cases
			if (checkInterfaces.Any(iface => collectionTypes.Contains(iface)))
				return true;

			//  checkType isn't generic, so can't possibly be IEnumerable<elementType>, it fails purity
			if (!checkType.IsGenericType)
				return false;

			return checkType == typeof(IEnumerable<>).MakeGenericType(elementType);
		}

		private static Type GetEnumerableElementType()
		{
			var type = typeof(T);
			var interfaces = type.GetInterfaces();
			var enumerableInterfaceType = interfaces
				.FirstOrDefault(q => q.IsGenericType && q.GetGenericTypeDefinition() == _enumerableCompareType);
			if (enumerableInterfaceType == null && type.IsGenericType && type.GetGenericTypeDefinition() == _enumerableCompareType)
				enumerableInterfaceType = type;
			if (enumerableInterfaceType == null)
				return type == typeof(IEnumerable) || interfaces.Contains(typeof(IEnumerable)) ? typeof(object) : null;

			return enumerableInterfaceType.GetGenericArguments()[0];
		}
	}
}
