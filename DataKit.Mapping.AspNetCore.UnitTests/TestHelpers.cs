using Microsoft.Extensions.DependencyInjection;
using System;

namespace DataKit.Mapping.AspNetCore.UnitTests
{
	public static class TestHelpers
	{
		public static IServiceProvider CreateContainerWithMappingServices(
			Action<IServiceCollection> addAdditionalServices = null
			)
		{
			var services = new ServiceCollection();
			services.AddMapping();
			addAdditionalServices?.Invoke(services);
			return services.BuildServiceProvider();
		}
	}
}
