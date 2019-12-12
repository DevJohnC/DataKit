using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataKit.Mapping.AspNetCore.UnitTests
{
	[TestClass]
	public class ObjectFactoryTests
	{
		[TestMethod]
		public void Can_Create_Instance_Without_Dependencies_From_IObjectFactory()
		{
			var container = TestHelpers.CreateContainerWithMappingServices();
			using (var scope = container.CreateScope())
			{
				var objectFactory = scope.ServiceProvider.GetRequiredService<IObjectFactory>();
				var obj = objectFactory.CreateInstance<DependencyClass>();
				Assert.IsNotNull(obj);
			}
		}

		[TestMethod]
		public void Can_Create_Instance_With_Dependencies_From_IObjectFactory()
		{
			var container = TestHelpers.CreateContainerWithMappingServices(
				services => services.AddScoped<DependencyClass>()
				);
			using (var scope = container.CreateScope())
			{
				var objectFactory = scope.ServiceProvider.GetRequiredService<IObjectFactory>();
				var obj = objectFactory.CreateInstance<ClassWithDependency>();
				Assert.IsNotNull(obj);
			}
		}

		private class DependencyClass { }

		private class ClassWithDependency
		{
			public ClassWithDependency(DependencyClass dependencyClass)
			{
				Assert.IsNotNull(dependencyClass);
			}
		}
	}
}
