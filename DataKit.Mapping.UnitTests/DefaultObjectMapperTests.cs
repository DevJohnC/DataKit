using DataKit.Mapping.Binding;
using DataKit.Modelling.TypeModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataKit.Mapping.UnitTests
{
	[TestClass]
	public class DefaultObjectMapperTests
	{
		[TestMethod]
		public void Can_Inject_Single_Object()
		{
			var mapper = new DefaultObjectMapper();
			var src = new Source { Value = 2 };
			var trgt = new Target();
			mapper.Inject(src, trgt);
			Assert.AreEqual(src.Value, trgt.Value);
		}

		[TestMethod]
		public void Can_Inject_Enumerations()
		{
			var mapper = new DefaultObjectMapper();
			var src = new[] { new Source { Value = 2 } };
			var trgt = new[] { new Target() };
			mapper.InjectAll(src, trgt);
			Assert.AreEqual(src[0].Value, trgt[0].Value);
		}

		[TestMethod]
		public void Can_Map_Single_Object()
		{
			var mapper = new DefaultObjectMapper();
			var src = new Source { Value = 2 };
			var trgt = mapper.Map<Source, Target>(src);
			Assert.AreEqual(src.Value, trgt.Value);
		}

		[TestMethod]
		public void Can_Map_Enumerations()
		{
			var mapper = new DefaultObjectMapper();
			var src = new[] { new Source { Value = 2 } };
			var trgt = mapper.MapAll<Source, Target>(src)
				.ToArray();
			Assert.AreEqual(src.Length, trgt.Length);
			Assert.AreEqual(src[0].Value, trgt[0].Value);
		}

		[TestMethod]
		public void Can_Use_Custom_ObjectFactory()
		{
			var mapper = new DefaultObjectMapper(
				objectFactory: new TargetSubClassFactory()
				);
			var src = new Source { Value = 2 };
			var trgt = mapper.Map<Source, Target>(src);
			Assert.IsInstanceOfType(trgt, typeof(TargetSubClass));
		}

		[TestMethod]
		public void Can_Use_Custom_Bindings()
		{
			var mapper = new DefaultObjectMapper(
				bindingFactory: new DoublesValueBindingFactory()
				);
			var src = new Source { Value = 2 };
			var trgt = mapper.Map<Source, Target>(src);
			Assert.AreEqual(src.Value * 2, trgt.Value);
		}

		private class Source
		{
			public int Value { get; set; }
		}

		private class Target
		{
			public int Value { get; set; }
		}

		private class TargetSubClass : Target { }

		private class TargetSubClassFactory : IObjectFactory
		{
			public object CreateInstance(Type type)
			{
				return new TargetSubClass();
			}
		}

		private class DoublesValueBindingFactory : DefaultObjectMapper.IBindingFactory
		{
			public DataModelBinding<TypeModel<TFrom>, PropertyField, TypeModel<TTo>, PropertyField> GetBindingForMapping<TFrom, TTo>()
				where TFrom : class
				where TTo : class
			{
				return new TypeBindingBuilder<Source, Target>()
					.Bind(t => t.Value, s => s.Value, value => value * 2)
					.BuildBinding()
					as DataModelBinding<TypeModel<TFrom>, PropertyField, TypeModel<TTo>, PropertyField>;
			}
		}
	}
}
