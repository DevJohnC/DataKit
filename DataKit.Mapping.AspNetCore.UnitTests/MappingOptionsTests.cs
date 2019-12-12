using DataKit.Mapping.Binding;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace DataKit.Mapping.AspNetCore.UnitTests
{
	[TestClass]
	public class MappingOptionsTests
	{
		[TestMethod]
		public void Can_Add_Binding_Override_From_Assembly()
		{
			var mappingOptions = new MappingOptions();
			mappingOptions.AddBindingOverridesFromAssembly(GetType().Assembly);
			Assert.IsTrue(
				mappingOptions.Bindings.OfType<BindingApplicator<Source, Target>>().Any()
				);
		}

		[TestMethod]
		public void Can_Filter_Binding_Override_From_Assembly()
		{
			var mappingOptions = new MappingOptions();
			mappingOptions.AddBindingOverridesFromAssembly(GetType().Assembly, t => false);
			Assert.IsFalse(
				mappingOptions.Bindings.OfType<BindingApplicator<Source, Target>>().Any()
				);
		}

		private class TestBindingOverride : IBindingOverride<Source, Target>
		{
			public void BindFields(TypeBindingBuilder<Source, Target> builder)
			{
				throw new NotImplementedException();
			}
		}

		private class Source { }
		private class Target { }
	}
}
