using DataKit.Modelling;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DataKit.Mapping
{
	public class DefaultObjectFactory : IObjectFactory
	{
		private delegate object Factory();

		public static DefaultObjectFactory Instance { get; }
			= new DefaultObjectFactory();

		private readonly CachedCollection<Type, Type, Factory> _cache
			= new CachedCollection<Type, Type, Factory>(CreateFactory);

		public object CreateInstance(Type type)
			=> _cache.GetOrCreate(type, type)();

		private static Factory CreateFactory(Type type)
		{
			var ctor = type.GetConstructors(
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
				)
				.FirstOrDefault(
					q => q.GetParameters().Length == 0
				);

			if (ctor == null)
				throw new Exception($"Couldn't find parameterless constructor for type `{type.FullName}`.");

			return Expression.Lambda<Factory>(
				Expression.New(ctor)
				).Compile();
		}
	}
}
