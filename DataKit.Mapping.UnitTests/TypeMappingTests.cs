using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataKit.Mapping.Binding;
using System;

namespace DataKit.Mapping.UnitTests
{
	[TestClass]
	public class TypeMappingTests
	{
		[TestMethod]
		public void Can_Map_Top_Values_Between_Objects()
		{
			var mapping = new TypeBindingBuilder<Source, Target>()
				.Bind(t => t.FlatValue, s => s.FlatValue)
				.BuildBinding()
				.BuildMapping();

			var source = new Source { FlatValue = "Hello World" };
			var target = new Target();

			var sourceReader = new ObjectDataModelReader<Source>(source);
			var targetWriter = new ObjectDataModelWriter<Target>(target);

			mapping.Run(sourceReader, targetWriter);

			Assert.AreEqual(source.FlatValue, target.FlatValue);
		}

		[TestMethod]
		public void Can_Map_Deep_Values_Between_Objects()
		{
			var mapping = new TypeBindingBuilder<Source, Target>()
				.Bind(t => t.Nested.FlatValue, s => s.Nested.FlatValue)
				.BuildBinding()
				.BuildMapping();

			var source = new Source { Nested = new Source { FlatValue = "Hello World" } };
			var target = new Target();

			var sourceReader = new ObjectDataModelReader<Source>(source);
			var targetWriter = new ObjectDataModelWriter<Target>(target);

			mapping.Run(sourceReader, targetWriter);

			Assert.AreEqual(source.Nested.FlatValue, target.Nested.FlatValue);
		}

		[TestMethod]
		public void Can_Map_Top_Value_To_Deep_Value_Between_Objects()
		{
			var mapping = new TypeBindingBuilder<Source, Target>()
				.Bind(t => t.Nested.FlatValue, s => s.FlatValue)
				.BuildBinding()
				.BuildMapping();

			var source = new Source { FlatValue = "Hello World" };
			var target = new Target();

			var sourceReader = new ObjectDataModelReader<Source>(source);
			var targetWriter = new ObjectDataModelWriter<Target>(target);

			mapping.Run(sourceReader, targetWriter);

			Assert.AreEqual(source.FlatValue, target.Nested.FlatValue);
		}

		[TestMethod]
		public void Can_Map_Deep_Value_To_Top_Value_Between_Objects()
		{
			var mapping = new TypeBindingBuilder<Source, Target>()
				.Bind(t => t.FlatValue, s => s.Nested.FlatValue)
				.BuildBinding()
				.BuildMapping();

			var source = new Source { Nested = new Source { FlatValue = "Hello World" } };
			var target = new Target();

			var sourceReader = new ObjectDataModelReader<Source>(source);
			var targetWriter = new ObjectDataModelWriter<Target>(target);

			mapping.Run(sourceReader, targetWriter);

			Assert.AreEqual(source.Nested.FlatValue, target.FlatValue);
		}

		[TestMethod]
		public void Converts_Field_Values()
		{
			var mapping = new TypeBindingBuilder<Source, Target>()
				.Bind(t => t.ValueType, s => s.ValueType, value => value + 10)
				.BuildBinding()
				.BuildMapping();

			var source = new Source { ValueType = 10 };
			var target = new Target();

			var sourceReader = new ObjectDataModelReader<Source>(source);
			var targetWriter = new ObjectDataModelWriter<Target>(target);

			mapping.Run(sourceReader, targetWriter);

			Assert.AreEqual(source.ValueType + 10, target.ValueType);
		}

		[TestMethod]
		public void Ignores_Conversion_Failure()
		{
			var mapping = new TypeBindingBuilder<Source, Target>()
				.Bind(
					t => t.ValueType,
					s => s.ValueType,
					(int @in, out int @out) => { @out = @in; return false; }
				)
				.BuildBinding()
				.BuildMapping();

			var source = new Source { ValueType = 10 };
			var target = new Target();

			var sourceReader = new ObjectDataModelReader<Source>(source);
			var targetWriter = new ObjectDataModelWriter<Target>(target);

			mapping.Run(sourceReader, targetWriter);

			Assert.AreEqual(default, target.ValueType);
		}

		[TestMethod]
		public void Can_Map_Value_Types()
		{
			var mapping = new TypeBindingBuilder<Source, Target>()
				.Bind(t => t.ValueType, s => s.ValueType)
				.BuildBinding()
				.BuildMapping();

			var source = new Source { ValueType = 10 };
			var target = new Target();

			var sourceReader = new ObjectDataModelReader<Source>(source);
			var targetWriter = new ObjectDataModelWriter<Target>(target);

			mapping.Run(sourceReader, targetWriter);

			Assert.AreEqual(source.ValueType, target.ValueType);
		}

		private class Source
		{
			public string FlatValue { get; set; }

			public int ValueType { get; set; }

			public Source Nested { get; set; }
		}

		private class Target
		{
			public string FlatValue { get; set; }

			public int ValueType { get; set; }

			public Target Nested { get; set; }
		}
	}
}
