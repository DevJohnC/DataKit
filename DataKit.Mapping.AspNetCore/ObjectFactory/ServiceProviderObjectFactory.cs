using System;

namespace DataKit.Mapping.AspNetCore.ObjectFactory
{
	public class ServiceProviderObjectFactory : IObjectFactory
	{
		private readonly ServiceProviderObjectFactoryCache _factoryCache;
		private readonly IServiceProvider _serviceProvider;

		public ServiceProviderObjectFactory(
			ServiceProviderObjectFactoryCache factoryCache,
			IServiceProvider serviceProvider
			)
		{
			_factoryCache = factoryCache;
			_serviceProvider = serviceProvider;
		}

		public object CreateInstance(Type type)
		{
			return _factoryCache.GetOrCreateFactory(type)(_serviceProvider);
		}
	}
}
