using DataKit.Modelling;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace DataKit.Mapping.AspNetCore.ObjectFactory
{
	public class ServiceProviderObjectFactoryCache
	{
		private static Type[] _noAdditionalArgs = new Type[0];
		private static object[] _noArgs = new object[0];

		private readonly CachedCollection<Type, Type, Func<IServiceProvider, object>> _factoryCache
			= new CachedCollection<Type, Type, Func<IServiceProvider, object>>(CreateFactory);

		public Func<IServiceProvider, object> GetOrCreateFactory(Type type)
		{
			return _factoryCache.GetOrCreate(type, type);
		}

		private static Func<IServiceProvider, object> CreateFactory(Type type)
		{
			var factory = ActivatorUtilities.CreateFactory(type, _noAdditionalArgs);
			return (sP) => factory(sP, _noArgs);
		}
	}
}
