using Microsoft.Extensions.DependencyInjection;
using Silk.Data.SQL.Providers;
using Silk.Data.SQL.SQLite3;
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

		public static IDataProvider CreateTestProvider()
		{
			return new SQLite3DataProvider("Data Source=:memory:;Mode=Memory");
		}
	}
}
