using DataKit.SQL.Sqlite3;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DataKit.ORM.AspNetCore.UnitTests
{
	public static class TestHelpers
	{
		public static IServiceProvider CreateContainer(
			Action<IServiceCollection> addAdditionalServices = null
			)
		{
			var services = new ServiceCollection();
			addAdditionalServices?.Invoke(services);
			return services.BuildServiceProvider();
		}

		public static Sqlite3DataProvider CreateTestProvider()
		{
			return Sqlite3DataProvider.InMemory();
		}
	}
}
