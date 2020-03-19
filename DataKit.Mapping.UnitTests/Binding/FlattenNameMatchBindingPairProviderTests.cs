using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataKit.Mapping.Binding;
using DataKit.Modelling.TypeModels;
using System.Linq;

namespace DataKit.Mapping.UnitTests.Binding
{
	[TestClass]
	public class FlattenNameMatchBindingPairProviderTests
	{
		private readonly static TypeModel<SourceModel> SourceTypeModel = TypeModel.GetModelOf<SourceModel>();
		private readonly static TypeModel<TargetModel> TargetTypeModel = TypeModel.GetModelOf<TargetModel>();

		[TestMethod]
		public void GetBindingPairs_Includes_Candidates_With_Same_Flattened_Path_Left_To_Right()
		{
			var provider = new FlattenNameMatchBindingPairProvider<TypeModel, PropertyField, TypeModel, PropertyField>();
			var bindingPairs = provider.GetBindingPairs(
				SourceTypeModel, TargetTypeModel);
			var testPairExists = bindingPairs.Any(candidate =>
				string.Join(".", candidate.Source.Path) == "SubProperty" &&
				string.Join(".", candidate.Target.Path) == "Sub.Property"
			);
			Assert.IsTrue(testPairExists, "No binding pair with identical flat paths found.");
		}

		[TestMethod]
		public void GetBindingPairs_Includes_Candidates_With_Same_Flattened_Path_Right_To_Left()
		{
			var provider = new FlattenNameMatchBindingPairProvider<TypeModel, PropertyField, TypeModel, PropertyField>();
			var bindingPairs = provider.GetBindingPairs(
				TargetTypeModel, SourceTypeModel);
			var testPairExists = bindingPairs.Any(candidate =>
				string.Join(".", candidate.Target.Path) == "SubProperty" &&
				string.Join(".", candidate.Source.Path) == "Sub.Property"
			);
			Assert.IsTrue(testPairExists, "No binding pair with identical flat paths found.");
		}

		[TestMethod]
		public void GetBindingPairs_Ignores_Candidates_With_Mismatched_Flattened_Path()
		{
			var provider = new FlattenNameMatchBindingPairProvider<TypeModel, PropertyField, TypeModel, PropertyField>();
			var bindingPairs = provider.GetBindingPairs(
				SourceTypeModel, TargetTypeModel);
			var mismatchedExists = bindingPairs.Any(candidate =>
				string.Join(".", candidate.Source.Path) == "SubProperty" &&
				string.Join(".", candidate.Target.Path) == "Sub.Mismatched"
			);
			Assert.IsFalse(mismatchedExists, "Binding pair with mismatched flat paths found.");
		}

		[TestMethod]
		public void GetBindingPairs_Stops_At_MaxDepth()
		{
			var candidateSource = new FlattenNameMatchBindingPairProvider<TypeModel, PropertyField, TypeModel, PropertyField>
			{
				MaxDepth = 1
			};
			var bindingPairs = candidateSource.GetBindingPairs(
				SourceTypeModel, TargetTypeModel);
			var deepBindingExists = bindingPairs.Any(candidate =>
				string.Join(".", candidate.Source.Path) == "SubProperty" &&
				string.Join(".", candidate.Target.Path) == "Sub.Property"
			);
			Assert.IsFalse(deepBindingExists, "Deep binding pair shouldn't exist");
		}

		private class SourceModel
		{
			public string SubProperty { get; set; }
		}

		private class TargetModel
		{
			public SubTargetModel Sub { get; set; }
		}

		private class SubTargetModel
		{
			public string Property { get; set; }
			public string Mismatched { get; set; }
		}
	}
}
