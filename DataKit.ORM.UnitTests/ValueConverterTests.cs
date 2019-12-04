using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataKit.ORM.Schema;
using DataKit.ORM.Schema.Sql;
using DataKit.ORM.Sql.Expressions;
using Silk.Data.SQL.Expressions;
using System.Linq;

namespace DataKit.ORM.UnitTests
{
	[TestClass]
	public class ValueConverterTests
	{
		[TestMethod]
		public void Convert_EntityTable_Returns_TableExpression()
		{
			var dataModel = BuildDataSet<TestEntity>();
			var converter = new ValueConverter<TestEntity>(dataModel, null);
			var valueExpression = converter.ConvertExpression(entity => entity);
			var testExpression = valueExpression.QueryExpression as AliasReferenceQueryExpression;
			Assert.IsNotNull(testExpression);
			Assert.AreEqual(dataModel.StorageModel.DefaultTableName, testExpression.AliasIdentifier.AliasIdentifier);
		}

		[TestMethod]
		public void Convert_Constant_Returns_ValueExpression()
		{
			var dataModel = BuildDataSet<TestEntity>();
			var converter = new ValueConverter<TestEntity>(dataModel, null);
			var valueExpression = converter.ConvertExpression(q => true);

			var checkExpression = valueExpression.QueryExpression as ValueExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(true, checkExpression.Value);
		}

		[TestMethod]
		public void Convert_Variable_Returns_ValueExpression()
		{
			var dataModel = BuildDataSet<TestEntity>();
			var converter = new ValueConverter<TestEntity>(dataModel, null);
			var variable = true;
			var valueExpression = converter.ConvertExpression(q => variable);

			var checkExpression = valueExpression.QueryExpression as ValueExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(true, checkExpression.Value);
		}

		[TestMethod]
		public void Convert_Property_Returns_ValueExpression()
		{
			var dataModel = BuildDataSet<TestEntity>();
			var converter = new ValueConverter<TestEntity>(dataModel, null);
			var obj = new { Value = true };
			var valueExpression = converter.ConvertExpression(q => obj.Value);

			var checkExpression = valueExpression.QueryExpression as ValueExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(obj.Value, checkExpression.Value);
		}

		[TestMethod]
		public void Convert_Method_Parameter_Returns_ValueExpression()
		{
			var dataModel = BuildDataSet<TestEntity>();
			var converter = new ValueConverter<TestEntity>(dataModel, null);
			var valueExpression = InternalConvert(true);

			var checkExpression = valueExpression.QueryExpression as ValueExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(true, checkExpression.Value);

			SqlValueExpression<TestEntity, bool> InternalConvert(bool param)
			{
				return converter.ConvertExpression(q => param);
			}
		}

		[TestMethod]
		public void Convert_Local_Column_Returns_ColumnExpression()
		{
			var dataModel = BuildDataSet<TestEntity>();
			var converter = new ValueConverter<TestEntity>(dataModel, null);
			var valueExpression = converter.ConvertExpression(q => q.Int);

			var checkExpression = valueExpression.QueryExpression as ColumnWithAliasSourceQueryExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(dataModel.StorageModel[nameof(TestEntity.Int)].ColumnName, checkExpression.ColumnName);
		}

		[TestMethod]
		public void Convert_Embedded_Column_Returns_ColumnExpression()
		{
			var dataModel = BuildDataSet<TestEntity>();
			var converter = new ValueConverter<TestEntity>(dataModel, null);
			var valueExpression = converter.ConvertExpression(q => q.Child1.Int);

			var checkExpression = valueExpression.QueryExpression as ColumnWithAliasSourceQueryExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual("Child1_Int", checkExpression.ColumnName);

			valueExpression = converter.ConvertExpression(q => q.Child2.Int);

			checkExpression = valueExpression.QueryExpression as ColumnWithAliasSourceQueryExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual("Child2_Int", checkExpression.ColumnName);
		}

		[TestMethod]
		public void Convert_Foreign_Field_To_Joined_ColumnExpression()
		{
			var (dataModel, _) = BuildDataSet<TestEntity, TestChildEntity>();
			var converter = new ValueConverter<TestEntity>(dataModel, null);
			var valueExpression = converter.ConvertExpression(q => q.Child1.Int);

			Assert.IsTrue(valueExpression.RequiresJoins);
			var checkExpression = valueExpression.QueryExpression as ColumnWithAliasSourceQueryExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(nameof(TestEntity.Child1.Int), checkExpression.ColumnName);
			Assert.AreEqual($"_{nameof(TestEntity.Child1)}", checkExpression.SourceIdentifier.AliasIdentifier);

			valueExpression = converter.ConvertExpression(q => q.Child2.Int);

			Assert.IsTrue(valueExpression.RequiresJoins);
			checkExpression = valueExpression.QueryExpression as ColumnWithAliasSourceQueryExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(nameof(TestEntity.Child2.Int), checkExpression.ColumnName);
			Assert.AreEqual($"_{nameof(TestEntity.Child2)}", checkExpression.SourceIdentifier.AliasIdentifier);
		}

		//[TestMethod]
		//public void Convert_QueryBuilder_Returns_Built_QueryExpression()
		//{
		//	var dataModel = BuildDataSet<TestEntity>();
		//	var converter = new ValueConverter<TestEntity>(dataModel, null);
		//	var testBuilder = new TestQueryBuilder();
		//	var valueExpression = converter.ConvertExpression(q => testBuilder);

		//	Assert.ReferenceEquals(testBuilder.QueryExpression, valueExpression.QueryExpression);
		//}

		[TestMethod]
		public void Convert_AreEqual_Operator_Returns_ComparisonExpression()
		{
			var dataModel = BuildDataSet<TestEntity>();
			var converter = new ValueConverter<TestEntity>(dataModel, null);
			var valueExpression = converter.ConvertExpression(q => q.Child1.Int == q.Child2.Int);

			var checkExpression = valueExpression.QueryExpression as ComparisonExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ComparisonOperator.AreEqual, checkExpression.Operator);
		}

		[TestMethod]
		public void Convert_AreNotEqual_Operator_Returns_ComparisonExpression()
		{
			var dataModel = BuildDataSet<TestEntity>();
			var converter = new ValueConverter<TestEntity>(dataModel, null);
			var valueExpression = converter.ConvertExpression(q => q.Child1.Int != q.Child2.Int);

			var checkExpression = valueExpression.QueryExpression as ComparisonExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ComparisonOperator.AreNotEqual, checkExpression.Operator);
		}

		[TestMethod]
		public void Convert_GreaterThan_Operator_Returns_ComparisonExpression()
		{
			var dataModel = BuildDataSet<TestEntity>();
			var converter = new ValueConverter<TestEntity>(dataModel, null);
			var valueExpression = converter.ConvertExpression(q => q.Child1.Int > q.Child2.Int);

			var checkExpression = valueExpression.QueryExpression as ComparisonExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ComparisonOperator.GreaterThan, checkExpression.Operator);
		}

		[TestMethod]
		public void Convert_GreaterThanOrEqualTo_Operator_Returns_ComparisonExpression()
		{
			var dataModel = BuildDataSet<TestEntity>();
			var converter = new ValueConverter<TestEntity>(dataModel, null);
			var valueExpression = converter.ConvertExpression(q => q.Child1.Int >= q.Child2.Int);

			var checkExpression = valueExpression.QueryExpression as ComparisonExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ComparisonOperator.GreaterThanOrEqualTo, checkExpression.Operator);
		}

		[TestMethod]
		public void Convert_LessThan_Operator_Returns_ComparisonExpression()
		{
			var dataModel = BuildDataSet<TestEntity>();
			var converter = new ValueConverter<TestEntity>(dataModel, null);
			var condition = converter.ConvertExpression(q => q.Child1.Int < q.Child2.Int);

			var checkExpression = condition.QueryExpression as ComparisonExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ComparisonOperator.LessThan, checkExpression.Operator);
		}

		[TestMethod]
		public void Convert_LessThanOrEqualTo_Operator_Returns_ComparisonExpression()
		{
			var dataModel = BuildDataSet<TestEntity>();
			var converter = new ValueConverter<TestEntity>(dataModel, null);
			var valueExpression = converter.ConvertExpression(q => q.Child1.Int <= q.Child2.Int);

			var checkExpression = valueExpression.QueryExpression as ComparisonExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ComparisonOperator.LessThanOrEqualTo, checkExpression.Operator);
		}

		[TestMethod]
		public void Convert_Addition_Operator_Returns_ArithmaticExpression()
		{
			var dataModel = BuildDataSet<TestEntity>();
			var converter = new ValueConverter<TestEntity>(dataModel, null);
			var valueExpression = converter.ConvertExpression(q => q.Child1.Int + q.Child2.Int);

			var checkExpression = valueExpression.QueryExpression as ArithmaticQueryExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ArithmaticOperator.Addition, checkExpression.Operator);
		}

		[TestMethod]
		public void Convert_Subtraction_Operator_Returns_ArithmaticExpression()
		{
			var dataModel = BuildDataSet<TestEntity>();
			var converter = new ValueConverter<TestEntity>(dataModel, null);
			var valueExpression = converter.ConvertExpression(q => q.Child1.Int - q.Child2.Int);

			var checkExpression = valueExpression.QueryExpression as ArithmaticQueryExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ArithmaticOperator.Subtraction, checkExpression.Operator);
		}

		[TestMethod]
		public void Convert_Multiplication_Operator_Returns_ArithmaticExpression()
		{
			var dataModel = BuildDataSet<TestEntity>();
			var converter = new ValueConverter<TestEntity>(dataModel, null);
			var valueExpression = converter.ConvertExpression(q => q.Child1.Int * q.Child2.Int);

			var checkExpression = valueExpression.QueryExpression as ArithmaticQueryExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ArithmaticOperator.Multiplication, checkExpression.Operator);
		}

		[TestMethod]
		public void Convert_Division_Operator_Returns_ArithmaticExpression()
		{
			var dataModel = BuildDataSet<TestEntity>();
			var converter = new ValueConverter<TestEntity>(dataModel, null);
			var valueExpression = converter.ConvertExpression(q => q.Child1.Int / q.Child2.Int);

			var checkExpression = valueExpression.QueryExpression as ArithmaticQueryExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ArithmaticOperator.Division, checkExpression.Operator);
		}

		[TestMethod]
		public void Convert_BitwiseAnd_Operator_Returns_BitwiseOperationExpression()
		{
			var dataModel = BuildDataSet<TestEntity>();
			var converter = new ValueConverter<TestEntity>(dataModel, null);
			var valueExpression = converter.ConvertExpression(q => q.Child1.Int & q.Child2.Int);

			var checkExpression = valueExpression.QueryExpression as BitwiseOperationQueryExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(BitwiseOperator.And, checkExpression.Operator);
		}

		[TestMethod]
		public void Convert_BitwiseOr_Operator_Returns_BitwiseOperationExpression()
		{
			var dataModel = BuildDataSet<TestEntity>();
			var converter = new ValueConverter<TestEntity>(dataModel, null);
			var valueExpression = converter.ConvertExpression(q => q.Child1.Int | q.Child2.Int);

			var checkExpression = valueExpression.QueryExpression as BitwiseOperationQueryExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(BitwiseOperator.Or, checkExpression.Operator);
		}

		[TestMethod]
		public void Convert_BitwiseXor_Operator_Returns_BitwiseOperationExpression()
		{
			var dataModel = BuildDataSet<TestEntity>();
			var converter = new ValueConverter<TestEntity>(dataModel, null);
			var valueExpression = converter.ConvertExpression(q => q.Child1.Int ^ q.Child2.Int);

			var checkExpression = valueExpression.QueryExpression as BitwiseOperationQueryExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(BitwiseOperator.ExclusiveOr, checkExpression.Operator);
		}

		[TestMethod]
		public void Convert_And_Logical_Operator_Returns_ConditionExpression()
		{
			var dataModel = BuildDataSet<TestEntity>();
			var converter = new ValueConverter<TestEntity>(dataModel, null);
			var valueExpression = converter.ConvertExpression(q => q.Child1.Int == 1 && q.Child2.Int == 1);

			var checkExpression = valueExpression.QueryExpression as ConditionExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ConditionType.AndAlso, checkExpression.ConditionType);
		}

		[TestMethod]
		public void Convert_Or_Logical_Operator_Returns_ConditionExpression()
		{
			var dataModel = BuildDataSet<TestEntity>();
			var converter = new ValueConverter<TestEntity>(dataModel, null);
			var valueExpression = converter.ConvertExpression(q => q.Child1.Int == 1 || q.Child2.Int == 1);

			var checkExpression = valueExpression.QueryExpression as ConditionExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ConditionType.OrElse, checkExpression.ConditionType);
		}

		[TestMethod]
		public void Convert_Enum_Value_Returns_Integer_ValueExpression()
		{
			var dataModel = BuildDataSet<TestEntity>();
			var converter = new ValueConverter<TestEntity>(dataModel, null);
			var valueExpression = converter.ConvertExpression(q => TestEnum.ValueTwo);

			var checkExpression = valueExpression.QueryExpression as ValueExpression;
			Assert.IsNotNull(checkExpression);
			Assert.IsInstanceOfType(checkExpression.Value, typeof(int));
		}

		//[TestMethod]
		//public void Convert_Like_Function_Returns_Like_ComparisonExpression()
		//{
		//	var dataModel = BuildDataSet<TestEntity>();
		//	var converter = new ValueConverter<TestEntity>(dataModel, null);
		//	var valueExpression = converter.ConvertExpression(q => DatabaseFunctions.Like(q.A, "%search%"));

		//	var checkExpression = valueExpression.QueryExpression as ComparisonExpression;
		//	Assert.IsNotNull(checkExpression);
		//	Assert.AreEqual(ComparisonOperator.Like, checkExpression.Operator);
		//}

		[TestMethod]
		public void Convert_Foreign_PrimaryKey_Returns_Local_ForeignKey()
		{
			var (dataModel, _) = BuildDataSet<TestEntity, TestChildEntity>();
			var converter = new ValueConverter<TestEntity>(dataModel, null);
			var valueExpression = converter.ConvertExpression(q => q.Child1.Id);

			var checkExpression = valueExpression.QueryExpression as ColumnWithAliasSourceQueryExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual("Child1_Id", checkExpression.ColumnName);
			Assert.IsFalse(valueExpression.RequiresJoins);

			valueExpression = converter.ConvertExpression(q => q.Child2.Id);

			checkExpression = valueExpression.QueryExpression as ColumnWithAliasSourceQueryExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual("Child2_Id", checkExpression.ColumnName);
			Assert.IsFalse(valueExpression.RequiresJoins);
		}

		private SqlDataModel<T> BuildDataSet<T>()
			where T : class
			=> new DataSchemaBuilder()
				.AddSqlEntity<T>()
				.Build()
				.Sql.SqlEntities
				.OfType<SqlDataModel<T>>()
				.First();

		private (SqlDataModel<T1>, SqlDataModel<T2>) BuildDataSet<T1, T2>()
			where T1 : class
			where T2 : class
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<T1>()
				.AddSqlEntity<T2>()
				.Build();
			return (
				schema.Sql.SqlEntities.OfType<SqlDataModel<T1>>().First(),
				schema.Sql.SqlEntities.OfType<SqlDataModel<T2>>().First()
				);
		}

		private class TestEntity
		{
			public int Int { get; set; }

			public TestEnum Enum { get; set; }

			public TestChildEntity Child1 { get; set; }

			public TestChildEntity Child2 { get; set; }
		}

		private class TestChildEntity
		{
			public int Id { get; set; }

			public int Int { get; set; }
		}

		private enum TestEnum
		{
			ValueOne,
			ValueTwo
		}
	}
}
