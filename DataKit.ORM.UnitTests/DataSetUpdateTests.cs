using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataKit.ORM.Schema;
using DataKit.ORM.Schema.Sql;
using Silk.Data.SQL;
using Silk.Data.SQL.Expressions;
using System.Linq;

namespace DataKit.ORM.UnitTests
{
	[TestClass]
	public class DataSetUpdateTests
	{
		[TestMethod]
		public void Can_Update_With_Expression()
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
					QueryExpression.DefineColumn("Value", SqlDataType.Text(), isNullable: true)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value" },
					new object[] { "Hello" }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				dataSet.Update().Set(q => q.Value, q => "World").Execute();

				using (var result = dataProvider.ExecuteReader(QueryExpression.Select(QueryExpression.All(), from: QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName))))
				{
					Assert.IsTrue(result.HasRows);
					Assert.IsTrue(result.Read());
					Assert.AreEqual("World", result.GetString(1));
				}
			}
		}

		[TestMethod]
		public void Can_Update_With_Value()
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
					QueryExpression.DefineColumn("Value", SqlDataType.Text(), isNullable: true)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value" },
					new object[] { "Hello" }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				dataSet.Update().Set(q => q.Value, "World").Execute();

				using (var result = dataProvider.ExecuteReader(QueryExpression.Select(QueryExpression.All(), from: QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName))))
				{
					Assert.IsTrue(result.HasRows);
					Assert.IsTrue(result.Read());
					Assert.AreEqual("World", result.GetString(1));
				}
			}
		}

		[TestMethod]
		public void Can_Update_Entity_Instance()
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
					QueryExpression.DefineColumn("Value", SqlDataType.Text(), isNullable: true)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value" },
					new object[] { "Hello" },
					new object[] { "Hello" }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				var entity = new Flat_Entity { Id = 2, Value = "World" };
				dataSet.Update(entity).Execute();

				using (var result = dataProvider.ExecuteReader(QueryExpression.Select(QueryExpression.All(), from: QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName))))
				{
					Assert.IsTrue(result.HasRows);
					Assert.IsTrue(result.Read());
					Assert.AreEqual("Hello", result.GetString(1));
					Assert.IsTrue(result.Read());
					Assert.AreEqual("World", result.GetString(1));
				}
			}
		}

		[TestMethod]
		public void Can_Update_With_View()
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
					QueryExpression.DefineColumn("Value", SqlDataType.Text(), isNullable: true)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value" },
					new object[] { "Hello" }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				dataSet.Update(new { Value = "World" }).Execute();

				using (var result = dataProvider.ExecuteReader(QueryExpression.Select(QueryExpression.All(), from: QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName))))
				{
					Assert.IsTrue(result.HasRows);
					Assert.IsTrue(result.Read());
					Assert.AreEqual("World", result.GetString(1));
				}
			}
		}

		[TestMethod]
		public void Execute_Returns_Affected_Row_Count()
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
					QueryExpression.DefineColumn("Value", SqlDataType.Text(), isNullable: true)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value" },
					new object[] { "Hello" },
					new object[] { "Hello" }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				var result = dataSet.Update().Set(q => q.Value, "World").Execute();

				Assert.AreEqual(2, result);
			}
		}

		[TestMethod]
		public void Can_Update_Field_To_Null_With_Expression()
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
					QueryExpression.DefineColumn("Value", SqlDataType.Text(), isNullable: true)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value" },
					new object[] { "Hello" }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				dataSet.Update().Set(q => q.Value, q => null).Execute();

				using (var result = dataProvider.ExecuteReader(QueryExpression.Select(QueryExpression.All(), from: QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName))))
				{
					Assert.IsTrue(result.HasRows);
					Assert.IsTrue(result.Read());
					Assert.IsTrue(result.IsDBNull(1));
				}
			}
		}

		[TestMethod]
		public void Can_Update_Field_To_Null_With_Entity()
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
					QueryExpression.DefineColumn("Value", SqlDataType.Text(), isNullable: true)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value" },
					new object[] { "Hello" }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				var entity = new Flat_Entity { Id = 1, Value = null };
				dataSet.Update(entity).Execute();

				using (var result = dataProvider.ExecuteReader(QueryExpression.Select(QueryExpression.All(), from: QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName))))
				{
					Assert.IsTrue(result.HasRows);
					Assert.IsTrue(result.Read());
					Assert.IsTrue(result.IsDBNull(1));
				}
			}
		}

		[TestMethod]
		public void Can_Update_Field_To_Null_With_View()
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
					QueryExpression.DefineColumn("Value", SqlDataType.Text(), isNullable: true)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value" },
					new object[] { "Hello" }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				var view = new { Value = default(string) };
				dataSet.Update(view).AndWhere(q => q.Id == 1).Execute();

				using (var result = dataProvider.ExecuteReader(QueryExpression.Select(QueryExpression.All(), from: QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName))))
				{
					Assert.IsTrue(result.HasRows);
					Assert.IsTrue(result.Read());
					Assert.IsTrue(result.IsDBNull(1));
				}
			}
		}

		private class Flat_Entity
		{
			public int Id { get; set; }
			public string Value { get; set; }
		}
	}
}
