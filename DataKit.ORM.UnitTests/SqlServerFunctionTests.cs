using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataKit.ORM.Schema;
using DataKit.ORM.Schema.Sql;
using DataKit.ORM.Sql.Expressions;
using DataKit.ORM.Sql.Expressions.MethodConversion;
using Silk.Data.SQL;
using Silk.Data.SQL.Expressions;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace DataKit.ORM.UnitTests
{
	[TestClass]
	public class SqlServerFunctionTests
	{
		[TestMethod]
		public void Can_Select_LastInsertId()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isNullable: false, isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value" },
					new object[] { 1 },
					new object[] { 2 }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				var id = dataSet.Select(q => dataSet.ServerFunctions.LastInsertId()).ToSingle();
				Assert.AreEqual(2, id);
			}
		}

		[TestMethod]
		public void Can_Select_Random()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isNullable: false, isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value" },
					new object[] { 1 },
					new object[] { 2 }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				var rand = dataSet.Select(q => dataSet.ServerFunctions.Random()).ToSingle();
				Assert.AreNotEqual(0, rand);
			}
		}

		[TestMethod]
		public void Can_Select_Count()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isNullable: false, isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value" },
					new object[] { 1 },
					new object[] { 2 }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				var id = dataSet.Select(q => dataSet.ServerFunctions.Count()).ToSingle();
				Assert.AreEqual(2, id);
			}
		}

		[TestMethod]
		public void Can_Select_Count_With_Expression()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isNullable: false, isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value" },
					new object[] { 1 },
					new object[] { 2 }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				var id = dataSet.Select(q => dataSet.ServerFunctions.Count(q.Id)).ToSingle();
				Assert.AreEqual(2, id);
			}
		}

		[TestMethod]
		public void Can_Select_IsLike()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity<string>>(q => q.DefaultTableName = "TestTable")
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity<string>>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isNullable: false, isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("Value", SqlDataType.Text(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value" },
					new object[] { "This is like" },
					new object[] { "Not so much" }
					));

				var dataSet = new SqlDataSet<Flat_Entity<string>>(sqlDataModel, dataProvider);
				var valueSet = dataSet.Select(q => dataSet.ServerFunctions.IsLike(q.Value, "%is%")).ToList();
				Assert.IsTrue(new[] { true, false }.SequenceEqual(valueSet));
			}
		}

		[TestMethod]
		public void Can_Select_IsIn_With_New_Array()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isNullable: false, isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value" },
					new object[] { 1 },
					new object[] { 2 },
					new object[] { 3 }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				var valueSet = dataSet.Select(q => dataSet.ServerFunctions.IsIn(q.Id, new[] { 1, 3 })).ToList();
				Assert.IsTrue(new[] { true, false, true }.SequenceEqual(valueSet));
			}
		}

		[TestMethod]
		public void Can_Select_IsIn_With_Array_Variable()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isNullable: false, isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value" },
					new object[] { 1 },
					new object[] { 2 },
					new object[] { 3 }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				var isInArray = new[] { 1, 3 };
				var valueSet = dataSet.Select(q => dataSet.ServerFunctions.IsIn(q.Id, isInArray)).ToList();
				Assert.IsTrue(new[] { true, false, true }.SequenceEqual(valueSet));
			}
		}

		[TestMethod]
		public void Can_Select_IsIn_With_SubQuery()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isNullable: false, isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value" },
					new object[] { 1 },
					new object[] { 2 },
					new object[] { 3 }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				var subQuery = dataSet.Select(q => q.Id).AndWhere(q => q.Id == 1 || q.Id == 3);
				var valueSet = dataSet.Select(q => dataSet.ServerFunctions.IsIn(q.Id, subQuery)).ToList();
				Assert.IsTrue(new[] { true, false, true }.SequenceEqual(valueSet));
			}
		}

		[TestMethod]
		public void Can_Select_HasFlag_With_Value()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Enum_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Enum_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isNullable: false, isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false),
					QueryExpression.DefineColumn("Requirement", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value", "Requirement" },
					new object[] { 0x1, 0x1 },
					new object[] { 0x2, 0x1 },
					new object[] { 0x1 | 0x4, 0x1 }
					));

				var dataSet = new SqlDataSet<Enum_Entity>(sqlDataModel, dataProvider);
				var valueSet = dataSet.Select(q => dataSet.ServerFunctions.HasFlag(q.Value, MyEnum.A)).ToList();
				Assert.IsTrue(new[] { true, false, true }.SequenceEqual(valueSet));
			}
		}

		[TestMethod]
		public void Can_Select_Min()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isNullable: false, isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value" },
					new object[] { 1 },
					new object[] { 2 },
					new object[] { 3 }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				var value = dataSet.Select(q => dataSet.ServerFunctions.Min(q.Id)).ToSingle();
				Assert.AreEqual(1, value);
			}
		}

		[TestMethod]
		public void Can_Select_Max()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isNullable: false, isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value" },
					new object[] { 1 },
					new object[] { 2 },
					new object[] { 3 }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				var value = dataSet.Select(q => dataSet.ServerFunctions.Max(q.Id)).ToSingle();
				Assert.AreEqual(3, value);
			}
		}

		[TestMethod]
		public void Can_Select_HasFlag_With_Expression()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Enum_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Enum_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isNullable: false, isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false),
					QueryExpression.DefineColumn("Requirement", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value", "Requirement" },
					new object[] { 0x1, 0x1 },
					new object[] { 0x2, 0x1 },
					new object[] { 0x1 | 0x4, 0x1 }
					));

				var dataSet = new SqlDataSet<Enum_Entity>(sqlDataModel, dataProvider);
				var valueSet = dataSet.Select(q => dataSet.ServerFunctions.HasFlag(q.Value, q.Requirement)).ToList();
				Assert.IsTrue(new[] { true, false, true }.SequenceEqual(valueSet));
			}
		}

		[TestMethod]
		public void Can_Extend_Server_Method_Support()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlMethodCallConverter(new ServerFunctionConverter())
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isNullable: false, isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value" },
					new object[] { 1 },
					new object[] { 2 }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				var resultSet = dataSet.Select().AndWhere(q => dataSet.ServerFunctions.ValueIsGreaterThanOne(q)).ToList();
				Assert.AreEqual(1, resultSet.Count);
				Assert.AreEqual(2, resultSet[0].Value);
			}
		}

		public class Flat_Entity
		{
			public int Id { get; set; }

			public int Value { get; set; }
		}

		private class Flat_Entity<T>
		{
			public int Id { get; set; }
			public T Value { get; set; }
		}

		private class Enum_Entity
		{
			public int Id { get; private set; }

			public MyEnum Value { get; set; }

			public MyEnum Requirement { get; set; }
		}

		[Flags]
		private enum MyEnum
		{
			None	= 0x0,
			A	= 0x1,
			B	= 0x2,
			C	= 0x4,
			D	= 0x8
		}

		private class ServerFunctionConverter : ISqlMethodCallConverter
		{
			public bool TryConvertMethod<TEntity>(MethodCallExpression methodCallExpression, ExpressionConversionVisitor<TEntity> expressionConverter, out LinqQueryExpression<TEntity> convertedExpression) where TEntity : class
			{
				if (methodCallExpression.Method.Name != nameof(SqlServerFunctionsExtensions.ValueIsGreaterThanOne))
				{
					convertedExpression = default;
					return false;
				}

				var typedConverter = expressionConverter as ExpressionConversionVisitor<Flat_Entity>;
				convertedExpression = Convert(typedConverter, q => q.Value > 1) as LinqQueryExpression<TEntity>;
				return true;
			}

			private LinqQueryExpression<Flat_Entity> Convert(ExpressionConversionVisitor<Flat_Entity> converter, Expression<Func<Flat_Entity, bool>> expr)
			{
				var conditionConverter = new ConditionConverter<Flat_Entity>(
					converter.DataModel,
					converter.TableIdentifier
					);
				var converted = conditionConverter.ConvertClause(expr);
				return new LinqQueryExpression<Flat_Entity>(
					converted.QueryExpression, converted.Joins
					);
			}
		}
	}

	static class SqlServerFunctionsExtensions
	{
		public static bool ValueIsGreaterThanOne(this SqlServerFunctions s, SqlServerFunctionTests.Flat_Entity entity)
		{
			//  don't implement
			return false;
		}
	}
}
