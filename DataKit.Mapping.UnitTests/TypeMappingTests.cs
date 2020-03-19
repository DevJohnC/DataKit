using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataKit.Mapping.Binding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataKit.Mapping.UnitTests
{
	[TestClass]
	public class TypeMappingTests
	{
		[TestMethod]
		public void Can_Map_With_Nulls_And_Leave_Defaults()
		{
			var mapping = new TypeBindingBuilder<Source, Target>()
				.Bind(t => t.Nested.FlatValue, s => s.Nested.FlatValue) // will be null when mapped
				.Bind(t => t.ValueType, s => s.ValueType) // will be mapped after the null is hit
				.BuildBinding()
				.BuildMapping();

			var source = new Source { ValueType = 5 };
			var target = new Target();

			var sourceReader = new ObjectDataModelReader<Source>(source);
			var targetWriter = new ObjectDataModelWriter<Target>(target);

			mapping.Run(sourceReader, targetWriter);

			Assert.IsNull(target.Nested);
			Assert.AreEqual(source.ValueType, target.ValueType);
		}

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

		[TestMethod]
		public void Can_Map_ReadOnly_Collections()
		{
			var mapping = new TypeBindingBuilder<Source, Target>()
				.Bind(t => t.ReadOnlyCollection, s => s.ReadOnlyCollection)
				.BuildBinding()
				.BuildMapping();

			var source = new Source
			{
				ReadOnlyCollection = new List<CollectionItem> { new CollectionItem { Data = "Hello World" } }
			};
			var target = new Target();

			var sourceReader = new ObjectDataModelReader<Source>(source);
			var targetWriter = new ObjectDataModelWriter<Target>(target);

			mapping.Run(sourceReader, targetWriter);

			Assert.IsTrue(
				source.ReadOnlyCollection.Select(q => q.Data)
					.SequenceEqual(target.ReadOnlyCollection.Select(q => q.Data))
				);
		}

		[TestMethod]
		public void Can_Map_Enumerables()
		{
			var mapping = new TypeBindingBuilder<Source, Target>()
				.Bind(t => t.Enumerable, s => s.Enumerable)
				.BuildBinding()
				.BuildMapping();

			var source = new Source
			{
				Enumerable = new List<CollectionItem> { new CollectionItem { Data = "Hello World" } }
			};
			var target = new Target();

			var sourceReader = new ObjectDataModelReader<Source>(source);
			var targetWriter = new ObjectDataModelWriter<Target>(target);

			mapping.Run(sourceReader, targetWriter);

			Assert.IsTrue(
				source.Enumerable.Select(q => q.Data)
					.SequenceEqual(target.Enumerable.Select(q => q.Data))
				);
		}

		[TestMethod]
		public void Can_Map_With_Null_Collection()
		{
			var mapping = new TypeBindingBuilder<Source, Target>()
				.Bind(t => t.ReadOnlyCollection, s => s.ReadOnlyCollection)
				.BuildBinding()
				.BuildMapping();

			var source = new Source
			{
				ReadOnlyCollection = null
			};
			var target = new Target();

			var sourceReader = new ObjectDataModelReader<Source>(source);
			var targetWriter = new ObjectDataModelWriter<Target>(target);

			mapping.Run(sourceReader, targetWriter);

			Assert.IsNull(target.ReadOnlyCollection);
		}

		[TestMethod]
		public void Can_Map_Multiple_Enumerables()
		{
			var mapping = new TypeBindingBuilder<Source, Target>()
				.Bind(t => t.ReadOnlyCollection, s => s.ReadOnlyCollection)
				.Bind(t => t.Enumerable, s => s.Enumerable)
				.BuildBinding()
				.BuildMapping();

			var source = new Source
			{
				ReadOnlyCollection = new List<CollectionItem> { new CollectionItem { Data = "Hello World #1" } },
				Enumerable = new List<CollectionItem> { new CollectionItem { Data = "Hello World #2" } }
			};
			var target = new Target();

			var sourceReader = new ObjectDataModelReader<Source>(source);
			var targetWriter = new ObjectDataModelWriter<Target>(target);

			mapping.Run(sourceReader, targetWriter);

			Assert.IsTrue(
				source.ReadOnlyCollection.Select(q => q.Data)
					.SequenceEqual(target.ReadOnlyCollection.Select(q => q.Data))
				);

			Assert.IsTrue(
				source.Enumerable.Select(q => q.Data)
					.SequenceEqual(target.Enumerable.Select(q => q.Data))
				);
		}

		private class Source
		{
			public string FlatValue { get; set; }

			public int ValueType { get; set; }

			public Source Nested { get; set; }

			public IReadOnlyCollection<CollectionItem> ReadOnlyCollection { get; set; }

			public IEnumerable<CollectionItem> Enumerable { get; set; }
		}

		private class Target
		{
			public string FlatValue { get; set; }

			public int ValueType { get; set; }

			public Target Nested { get; set; }

			public IReadOnlyCollection<CollectionItem> ReadOnlyCollection { get; set; }

			public IEnumerable<CollectionItem> Enumerable { get; set; }
		}

		private class CollectionItem
		{
			public string Data { get; set; }
		}
	}
}
