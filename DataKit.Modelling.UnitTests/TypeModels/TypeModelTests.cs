using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataKit.Modelling.TypeModels;

namespace DataKit.Modelling.UnitTests.TypeModels
{
	[TestClass]
	public class TypeModelTests
	{
		private static TypeModel<TestClass> _modelOfTestClass = TypeModel.GetModelOf<TestClass>();

		[TestMethod]
		public void TypeModel_GetModelOf_Uses_Cache()
		{
			var modelOne = TypeModel.GetModelOf<TestClass>();
			var modelTwo = TypeModel.GetModelOf<TestClass>();

			Assert.ReferenceEquals(modelOne, modelTwo);
		}

		[TestMethod]
		public void TypeModel_GetModelOf_Excludes_Static_Properties()
		{
			Assert.IsFalse(_modelOfTestClass.TryGetField(nameof(TestClass.Public_Static_GetSet), out PropertyField _));
		}

		[TestMethod]
		public void TypeModel_GetModelOf_Excludes_Fields()
		{
			Assert.IsFalse(_modelOfTestClass.TryGetField(nameof(TestClass.Public_Field), out PropertyField _));
		}

		[TestMethod]
		public void TypeModel_GetModelOf_Includes_Public_GetSet_Properties()
		{
			Assert.IsTrue(_modelOfTestClass.TryGetField(nameof(TestClass.Public_GetSet), out PropertyField field));
			Assert.IsTrue(field.CanRead);
			Assert.IsTrue(field.CanWrite);
		}

		[TestMethod]
		public void TypeModel_GetModelOf_Includes_Public_Get_Only_Properties()
		{
			Assert.IsTrue(_modelOfTestClass.TryGetField(nameof(TestClass.Public_Get_Only), out PropertyField field));
			Assert.IsTrue(field.CanRead);
			Assert.IsFalse(field.CanWrite);
		}

		[TestMethod]
		public void TypeModel_GetModelOf_Includes_Public_Set_Only_Properties()
		{
			Assert.IsTrue(_modelOfTestClass.TryGetField(nameof(TestClass.Public_Set_Only), out PropertyField field));
			Assert.IsFalse(field.CanRead);
			Assert.IsTrue(field.CanWrite);
		}

		[TestMethod]
		public void TypeModel_GetModelOf_Includes_Public_Get_NonPublic_Set_Properties()
		{
			Assert.IsTrue(_modelOfTestClass.TryGetField(nameof(TestClass.Public_Get_Private_Set), out PropertyField field));
			Assert.IsTrue(field.CanRead);
			Assert.IsTrue(field.CanWrite);
		}

		[TestMethod]
		public void TypeModel_GetModelOf_Includes_NonPublic_Get_Public_Set_Properties()
		{
			Assert.IsTrue(_modelOfTestClass.TryGetField(nameof(TestClass.Private_Get_Public_Set), out PropertyField field));
			Assert.IsTrue(field.CanRead);
			Assert.IsTrue(field.CanWrite);
		}

		private class TestClass
		{
			public static string Public_Static_GetSet { get; set; }

			public string Public_GetSet { get; set; }

			public string Public_Get_Only { get => null; }

			public string Public_Set_Only { set { } }

			public string Public_Get_Private_Set { get; private set; }

			public string Private_Get_Public_Set { private get; set; }

			public string Public_Field = "";
		}
	}
}
