using DataKit.Mapping;
using DataKit.Mapping.AspNetCore;
using DataKit.Mapping.AspNetCore.Mapping;
using DataKit.Mapping.AspNetCore.ObjectFactory;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class StartupExtensions
	{
		public static IServiceCollection AddMapping(this IServiceCollection services)
		{
			//  requirements
			services.AddOptions<MappingOptions>();

			//  services for object construction using a scoped IoC container
			services.AddSingleton<ServiceProviderObjectFactoryCache>();
			services.AddScoped<IObjectFactory, ServiceProviderObjectFactory>();

			//  services for object mapping, mapper is scoped to take advantage of scoped IObjectFactory
			//  but mappings are cached in a singleton to prevent constant generation of mappings
			services.AddSingleton<MappingCache>();
			services.AddScoped<IObjectMapper, AspNetCoreMapper>();
			return services;
		}
	}
}
