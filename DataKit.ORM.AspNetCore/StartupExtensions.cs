using DataKit.Mapping;
using DataKit.Mapping.AspNetCore.Mapping;
using DataKit.ORM;
using DataKit.ORM.AspNetCore;
using DataKit.SQL.Providers;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class StartupExtensions
	{
		public static IServiceCollection AddDataContext<TDataContext>(this IServiceCollection services, IDataProvider dataProvider)
			where TDataContext : DataContext, new()
		{
			services.AddMapping();

			services.AddScoped<TDataContext>(
				sP => DataContext.Create<TDataContext>(dataProvider,
					new AspNetCoreDataContextOptions(sP.GetRequiredService<IObjectFactory>(), sP.GetRequiredService<BindingProvider>()))
				);
			services.AddScoped<DataContext>(sP => sP.GetRequiredService<TDataContext>());

			return services;
		}
	}
}
