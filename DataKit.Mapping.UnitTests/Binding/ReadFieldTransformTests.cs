using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataKit.Mapping.Binding;
using DataKit.Modelling.TypeModels;

namespace DataKit.Mapping.UnitTests.Binding
{
	[TestClass]
	public class ReadFieldTransformTests
	{
		[TestMethod]
		public void Transform_Returns_False_With_Nulls_In_Path()
		{
			var transform = BuildTransformation();
			var obj = new TestType
			{
				Recurse = null
			};
			var bindingContext = new BindingContext();
			var transformSuccess = transform(bindingContext, obj, out var testValue);
			Assert.IsFalse(transformSuccess);

			obj = new TestType
			{
				Recurse = new TestType
				{
					Recurse = null
				}
			};
			transformSuccess = transform(bindingContext, obj, out testValue);
			Assert.IsFalse(transformSuccess);
		}

		[TestMethod]
		public void Transform_Returns_True_With_No_Nulls_In_Path()
		{
			var transform = BuildTransformation();
			var obj = new TestType
			{
				Recurse = new TestType
				{
					Recurse = new TestType
					{
						Value = 5
					}
				}
			};
			var bindingContext = new BindingContext();
			var transformSuccess = transform(bindingContext, obj, out var testValue);
			Assert.IsTrue(transformSuccess);
			Assert.AreEqual(obj.Recurse.Recurse.Value, testValue);
		}

		private BindingTransformation BuildTransformation()
		{
			var model = TypeModel.GetModelOf<TestType>();
			return ReadFieldTransform.Create(
				model[nameof(TestType.Recurse)],
				model[nameof(TestType.Recurse)],
				model[nameof(TestType.Value)]
				);
		}

		private class TestType
		{
			public int Value { get; set; }

			public TestType Recurse { get; set; }
		}
	}
}
