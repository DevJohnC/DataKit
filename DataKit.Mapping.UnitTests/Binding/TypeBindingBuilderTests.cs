using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataKit.Mapping.Binding;
using DataKit.Modelling.TypeModels;
using System.Linq;

namespace DataKit.Mapping.UnitTests.Binding
{
	[TestClass]
	public class TypeBindingBuilderTests
	{
		[TestMethod]
		public void AutoBind_Binds_Identical_Values_Types()
		{
			var builder = new TypeBindingBuilder(typeof(Source), typeof(Target));
			var binding = builder.AutoBind().BuildBinding();
			Assert.IsTrue(
				ContainsBinding(new[] { nameof(Source.ValueType) }, new[] { nameof(Target.ValueType) }, binding)
				);
		}

		[TestMethod]
		public void AutoBind_Binds_String_Types()
		{
			var builder = new TypeBindingBuilder(typeof(Source), typeof(Target));
			var binding = builder.AutoBind().BuildBinding();
			Assert.IsTrue(
				ContainsBinding(new[] { nameof(Source.StringType) }, new[] { nameof(Target.StringType) }, binding)
				);
		}

		[TestMethod]
		public void Bind_Property_With_Expression_Binds()
		{
			var builder = new TypeBindingBuilder<Source, Target>();
			var binding = builder.Bind(t => t.Nested.StringType, s => s.Nested.StringType).BuildBinding();
			Assert.IsTrue(
				ContainsBinding(new[] {
						nameof(Source.Nested), nameof(Source.StringType)
					}, new[] {
						nameof(Target.Nested), nameof(Target.StringType)
					},
					binding)
				);
		}

		[TestMethod]
		public void Bind_Property_With_Expression_And_Converter_Binds()
		{
			var builder = new TypeBindingBuilder<Source, Target>();
			var binding = builder.Bind(
					t => t.Nested.StringType,
					s => s.Nested.ValueType,
					value => value.ToString())
				.BuildBinding();
			Assert.IsTrue(
				ContainsBinding(new[] {
						nameof(Source.Nested), nameof(Source.ValueType)
					}, new[] {
						nameof(Target.Nested), nameof(Target.StringType)
					},
					binding)
				);
		}

		[TestMethod]
		public void AutoBind_Uses_Existing_Binding()
		{
			var flatBinding = new TypeBindingBuilder<SourceFlat, TargetFlat>()
				.Bind(t => t.StringType, s => s.ValueType, s => s.ToString())
				.BuildBinding();

			var binding = new TypeBindingBuilder<Source, Target>()
				.AutoBind(new[] { flatBinding })
				.BuildBinding();

			Assert.IsTrue(
				ContainsBinding(new[] {
						nameof(Source.Flat), nameof(SourceFlat.ValueType)
					}, new[] {
						nameof(Target.Flat), nameof(TargetFlat.StringType)
					},
					binding)
				);
		}

		[TestMethod]
		public void Binding_Field_With_Binding()
		{
			var flatBinding = new TypeBindingBuilder<SourceFlat, TargetFlat>()
				.Bind(t => t.StringType, s => s.ValueType, s => s.ToString())
				.BuildBinding();

			var binding = new TypeBindingBuilder<Source, Target>()
				.Bind(t => t.Flat, s => s.Flat, flatBinding)
				.BuildBinding();

			Assert.IsTrue(
				ContainsBinding(new[] {
						nameof(Source.Flat), nameof(SourceFlat.ValueType)
					}, new[] {
						nameof(Target.Flat), nameof(TargetFlat.StringType)
					},
					binding)
				);
		}

		[TestMethod]
		public void Binding_Field_With_Binding_Ignores_Previously_Bound_Fields()
		{
			var flatBinding = new TypeBindingBuilder<SourceFlat, TargetFlat>()
				.Bind(t => t.StringType, s => s.ValueType, s => s.ToString())
				.BuildBinding();

			var binding = new TypeBindingBuilder<Source, Target>()
				.Bind(t => t.Flat.StringType, s => s.StringType)
				.Bind(t => t.Flat, s => s.Flat, flatBinding)
				.BuildBinding();

			Assert.IsTrue(
				ContainsBinding(new[] {
						nameof(Source.StringType)
					}, new[] {
						nameof(Target.Flat), nameof(TargetFlat.StringType)
					},
					binding)
				);

			Assert.IsFalse(
				ContainsBinding(new[] {
						nameof(Source.Flat), nameof(SourceFlat.ValueType)
					}, new[] {
						nameof(Target.Flat), nameof(TargetFlat.StringType)
					},
					binding)
				);
		}

		private bool ContainsBinding(
			string[] sourcePath, string[] targetPath,
			DataModelBinding<PropertyField, PropertyField> binding)
		{
			return binding.FieldBindings.Any(
				q => q.BindingSource.Path.SequenceEqual(sourcePath) &&
					q.BindingTarget.Path.SequenceEqual(targetPath)
				);
		}

		private class Source
		{
			public int ValueType { get; }
			public string StringType { get; }
			public Source Nested { get; }
			public SourceFlat Flat { get; }
		}

		private class SourceFlat
		{
			public int ValueType { get; }
		}

		private class Target
		{
			public int ValueType { get; set; }
			public string StringType { get; set; }
			public Target Nested { get; set; }
			public TargetFlat Flat { get; set; }
		}

		private class TargetFlat
		{
			public string StringType { get; set; }
		}
	}
}
