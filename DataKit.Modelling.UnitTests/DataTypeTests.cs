using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DataKit.Modelling.UnitTests
{
	[TestClass]
	public class DataTypeTests
	{
		[TestMethod]
		public void String_Is_Not_Pure_Enumerable()
		{
			var type = typeof(string);
			var dataType = DataType.GetTypeOf(type);
			Assert.IsTrue(dataType.IsEnumerable);
			Assert.IsFalse(dataType.IsPureEnumerable);
		}

		[TestMethod]
		public void Array_Is_Pure_Enumerable()
		{
			var type = typeof(int[]);
			var dataType = DataType.GetTypeOf(type);
			Assert.IsTrue(dataType.IsEnumerable);
			Assert.IsTrue(dataType.IsPureEnumerable);
		}

		[TestMethod]
		public void List_Is_Pure_Enumerable()
		{
			var type = typeof(List<int>);
			var dataType = DataType.GetTypeOf(type);
			Assert.IsTrue(dataType.IsEnumerable);
			Assert.IsTrue(dataType.IsPureEnumerable);
		}

		[TestMethod]
		public void Dictionary_Is_Pure_Enumerable()
		{
			var type = typeof(Dictionary<int,int>);
			var dataType = DataType.GetTypeOf(type);
			Assert.IsTrue(dataType.IsEnumerable);
			Assert.IsTrue(dataType.IsPureEnumerable);
		}

		[TestMethod]
		public void IEnumerable_Is_Pure_Enumerable()
		{
			var type = typeof(IEnumerable);
			var dataType = DataType.GetTypeOf(type);
			Assert.IsTrue(dataType.IsEnumerable);
			Assert.IsTrue(dataType.IsPureEnumerable);
		}

		[TestMethod]
		public void ICollection_Is_Pure_Enumerable()
		{
			var type = typeof(ICollection);
			var dataType = DataType.GetTypeOf(type);
			Assert.IsTrue(dataType.IsEnumerable);
			Assert.IsTrue(dataType.IsPureEnumerable);
		}

		[TestMethod]
		public void IList_Is_Pure_Enumerable()
		{
			var type = typeof(IList);
			var dataType = DataType.GetTypeOf(type);
			Assert.IsTrue(dataType.IsEnumerable);
			Assert.IsTrue(dataType.IsPureEnumerable);
		}

		[TestMethod]
		public void IDictionary_Is_Pure_Enumerable()
		{
			var type = typeof(IDictionary);
			var dataType = DataType.GetTypeOf(type);
			Assert.IsTrue(dataType.IsEnumerable);
			Assert.IsTrue(dataType.IsPureEnumerable);
		}

		[TestMethod]
		public void IEnumerable_Of_T_Is_Pure_Enumerable()
		{
			var type = typeof(IEnumerable<int>);
			var dataType = DataType.GetTypeOf(type);
			Assert.IsTrue(dataType.IsEnumerable);
			Assert.IsTrue(dataType.IsPureEnumerable);
		}

		[TestMethod]
		public void ICollection_Of_T_Is_Pure_Enumerable()
		{
			var type = typeof(ICollection<int>);
			var dataType = DataType.GetTypeOf(type);
			Assert.IsTrue(dataType.IsEnumerable);
			Assert.IsTrue(dataType.IsPureEnumerable);
		}

		[TestMethod]
		public void IReadOnlyCollection_Of_T_Is_Pure_Enumerable()
		{
			var type = typeof(IReadOnlyCollection<int>);
			var dataType = DataType.GetTypeOf(type);
			Assert.IsTrue(dataType.IsEnumerable);
			Assert.IsTrue(dataType.IsPureEnumerable);
		}

		[TestMethod]
		public void IList_Of_T_Is_Pure_Enumerable()
		{
			var type = typeof(IList<int>);
			var dataType = DataType.GetTypeOf(type);
			Assert.IsTrue(dataType.IsEnumerable);
			Assert.IsTrue(dataType.IsPureEnumerable);
		}

		[TestMethod]
		public void IReadOnlyList_Of_T_Is_Pure_Enumerable()
		{
			var type = typeof(IReadOnlyList<int>);
			var dataType = DataType.GetTypeOf(type);
			Assert.IsTrue(dataType.IsEnumerable);
			Assert.IsTrue(dataType.IsPureEnumerable);
		}

		[TestMethod]
		public void IDictionary_Of_T_Is_Pure_Enumerable()
		{
			var type = typeof(IDictionary<int, int>);
			var dataType = DataType.GetTypeOf(type);
			Assert.IsTrue(dataType.IsEnumerable);
			Assert.IsTrue(dataType.IsPureEnumerable);
		}

		[TestMethod]
		public void IReadOnlyDictionary_Of_T_Is_Pure_Enumerable()
		{
			var type = typeof(IReadOnlyDictionary<int, int>);
			var dataType = DataType.GetTypeOf(type);
			Assert.IsTrue(dataType.IsEnumerable);
			Assert.IsTrue(dataType.IsPureEnumerable);
		}

		[TestMethod]
		public void List_Of_T_Is_Pure_Enumerable()
		{
			var type = typeof(List<int>);
			var dataType = DataType.GetTypeOf(type);
			Assert.IsTrue(dataType.IsEnumerable);
			Assert.IsTrue(dataType.IsPureEnumerable);
		}

		[TestMethod]
		public void Dictionary_Of_T_Is_Pure_Enumerable()
		{
			var type = typeof(Dictionary<int, int>);
			var dataType = DataType.GetTypeOf(type);
			Assert.IsTrue(dataType.IsEnumerable);
			Assert.IsTrue(dataType.IsPureEnumerable);
		}

		[TestMethod]
		public void Int_Is_Not_Enumerable()
		{
			var type = typeof(int);
			var dataType = DataType.GetTypeOf(type);
			Assert.IsFalse(dataType.IsEnumerable);
		}

		[TestMethod]
		public void String_Is_Not_Anonymous_Type()
		{
			var type = typeof(string);
			var dataType = DataType.GetTypeOf(type);
			Assert.IsFalse(dataType.IsAnonymousType);
		}

		[TestMethod]
		public void Anonymous_Type_Is_Anonymous_Type()
		{
			var type = GetAnonymousType(new { });
			var dataType = DataType.GetTypeOf(type);
			Assert.IsTrue(dataType.IsAnonymousType);
		}

		private Type GetAnonymousType<T>(T instance)
		{
			return typeof(T);
		}
	}
}
