using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataKit.Mapping.UnitTests
{
	[TestClass]
	public class DefaultObjectFactoryTests
	{
		[TestMethod]
		public void CreateInstance_Finds_Public_Ctor()
		{
			Assert.IsNotNull(
				DefaultObjectFactory.Instance.CreateInstance<PublicCtor>()
				);
		}

		[TestMethod]
		public void CreateInstance_Finds_Private_Ctor()
		{
			Assert.IsNotNull(
				DefaultObjectFactory.Instance.CreateInstance<PrivateCtor>()
				);
		}

		[TestMethod]
		public void CreateInstance_Chooses_Parameterless_Ctor()
		{
			Assert.IsNotNull(
				DefaultObjectFactory.Instance.CreateInstance<MultipleCtor>()
				);
		}

		private class PublicCtor { public PublicCtor() { } }

		private class PrivateCtor { private PrivateCtor() { } }

		private class MultipleCtor { private MultipleCtor() { } public MultipleCtor(int parameter1) { } }
	}
}
