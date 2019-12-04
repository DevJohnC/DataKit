using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace DataKit.Mapping.UnitTests.ObjectReadWriteApis
{
	[TestClass]
	public class ObjectManualMapTests
	{
		[TestMethod]
		public void Read_Write_Top_Level_Property()
		{
			var source = new SourceClass { Value = "Hello World" };
			var target = new TargetClass();

			var reader = new ObjectDataModelReader<SourceClass>(source);
			var writer = new ObjectDataModelWriter<TargetClass>(target);

			writer.WriteField(
				writer.Model[nameof(SourceClass.Value)],
				reader.ReadField<string>(reader.Model[nameof(TargetClass.Value)])
				);

			Assert.AreEqual(source.Value, target.Value);
		}

		[TestMethod]
		public void Read_Write_Nested_Property()
		{
			var source = new SourceClass
			{
				Value = "Hello World",
				Nested = new SourceClass
				{
					Value = "Hello Nested"
				}
			};
			var target = new TargetClass();

			var reader = new ObjectDataModelReader<SourceClass>(source);
			var writer = new ObjectDataModelWriter<TargetClass>(target);

			Assert.IsNotNull(reader.ReadField<SourceClass>(reader.Model[nameof(SourceClass.Nested)]));
			reader.EnterMember(reader.Model[nameof(SourceClass.Nested)]);
			writer.EnterMember(writer.Model[nameof(TargetClass.Nested)]);

			writer.WriteField(
				writer.Model[nameof(TargetClass.Value)],
				reader.ReadField<string>(reader.Model[nameof(SourceClass.Value)])
				);

			writer.LeaveMember();
			reader.LeaveMember();

			Assert.IsNotNull(target.Nested);
			Assert.AreEqual(source.Nested.Value, target.Nested.Value);
		}

		[TestMethod]
		public void Read_Write_Enumerable()
		{
			var source = new SourceClass
			{
				Enumerable = new[]
				{
					new SourceClass { Value = "Value 1" },
					new SourceClass { Value = "Value 2" }
				}
			};
			var target = new TargetClass();

			var reader = new ObjectDataModelReader<SourceClass>(source);
			var writer = new ObjectDataModelWriter<TargetClass>(target);

			reader.EnterMember(reader.Model[nameof(SourceClass.Enumerable)]);
			reader.EnterEnumerable();
			writer.EnterMember(writer.Model[nameof(TargetClass.Enumerable)]);
			writer.EnterEnumerable();

			while (reader.MoveNext())
			{
				writer.MoveNext();
				writer.WriteField(
					writer.Model[nameof(target.Value)],
					reader.ReadField<string>(reader.Model[nameof(SourceClass.Value)])
					);
			}

			writer.LeaveEnumerable();
			writer.LeaveMember();
			reader.LeaveEnumerable();
			reader.LeaveMember();

			Assert.IsNotNull(target.Enumerable);
			Assert.IsTrue(source.Enumerable.Select(q => q.Value).SequenceEqual(target.Enumerable.Select(q => q.Value)));
		}

		private class SourceClass
		{
			public string Value { get; set; }

			public SourceClass Nested { get; set; }

			public IEnumerable<SourceClass> Enumerable { get; set; }
		}

		private class TargetClass
		{
			public string Value { get; set; }

			public TargetClass Nested { get; set; }

			public IEnumerable<TargetClass> Enumerable { get; set; }
		}
	}
}
