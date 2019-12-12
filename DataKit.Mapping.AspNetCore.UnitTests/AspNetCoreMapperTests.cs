using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataKit.Mapping.AspNetCore.UnitTests
{
	[TestClass]
	public class AspNetCoreMapperTests
	{
		[TestMethod]
		public void Can_Map_To_Type_With_Scoped_Dependency()
		{
			var container = TestHelpers.CreateContainerWithMappingServices(
				services => services.AddScoped<TargetDependency>()
				);
			using (var scope = container.CreateScope())
			{
				var mapper = scope.ServiceProvider.GetRequiredService<IObjectMapper>();
				var src = new Source { Value = 2 };
				var trgt = mapper.Map<Source, Target>(src);
				Assert.AreEqual(src.Value, trgt.Value);
			}
		}

		[TestMethod]
		public void Can_Map_With_Binding_Override()
		{
			var container = TestHelpers.CreateContainerWithMappingServices(
				services => services.Configure<MappingOptions>(options => {
					options.AddBindingOverride<Source, TargetDependency>(
						builder => builder.Bind(t => t.OptionalValue, s => s.Value, value => value * 2)
						);
				}));
			using (var scope = container.CreateScope())
			{
				var mapper = scope.ServiceProvider.GetRequiredService<IObjectMapper>();
				var src = new Source { Value = 2 };
				var trgt = mapper.Map<Source, TargetDependency>(src);
				Assert.AreEqual(src.Value * 2, trgt.OptionalValue);
			}
		}

		private class Source
		{
			public int Value { get; set; }
		}

		private class Target
		{
			public int Value { get; set; }
			public TargetDependency Dependency { get; }

			public Target(TargetDependency targetDependency)
			{
				Assert.IsNotNull(targetDependency);
				Dependency = targetDependency;
			}
		}

		private class TargetDependency
		{
			public int OptionalValue { get; set; }
		}
	}
}
