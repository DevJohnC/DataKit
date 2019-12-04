using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataKit.Mapping.Binding;
using DataKit.ORM.Schema;
using DataKit.ORM.Schema.Sql;
using Silk.Data.SQL;
using Silk.Data.SQL.Expressions;
using System;
using System.Linq;

namespace DataKit.ORM.UnitTests
{
	[TestClass]
	public class DataSetSelectTests
	{
		[TestMethod]
		public void Can_Select_LastInsertedId_Entity()
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
				var valueSet = dataSet.Select().LastInsertId().ToSingle();

				Assert.AreEqual(2, valueSet.Id);
				Assert.AreEqual(default(int), valueSet.Value);
			}
		}

		[TestMethod]
		public void Can_Select_LastInsertedId_View()
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
				var valueSet = dataSet.Select<Flat_Entity_Id_View>().LastInsertId().ToSingle();

				Assert.AreEqual(2, valueSet.Id);
			}
		}

		[TestMethod]
		public void Select_Expression_Returns_Value()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value" },
					new object[] { 1 },
					new object[] { 2 }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				var valueSet = dataSet.Select(q => q.Value).ToList();

				Assert.IsTrue(
					valueSet.SequenceEqual(new[] { 1, 2 })
					);
			}
		}

		[TestMethod]
		public void Select_Expression_With_Join_Returns_Value()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Complex_Entity>()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Complex_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"Flat_Entity",
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false),
					QueryExpression.DefineColumn("Flat_Id", SqlDataType.Int(), isNullable: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					"Flat_Entity",
					new[] { "Value" },
					new object[] { 5 },
					new object[] { 6 }
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value", "Flat_Id" },
					new object[] { 3, 1 },
					new object[] { 4, 2 }
					));

				var dataSet = new SqlDataSet<Complex_Entity>(sqlDataModel, dataProvider);
				var valueSet = dataSet.Select(q => q.Flat.Value).ToList();

				Assert.IsTrue(
					valueSet.SequenceEqual(new[] { 5, 6 })
					);
			}
		}

		[TestMethod]
		public void Select_MultiExpression_With_Join_Returns_Zipped_DataSet()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Complex_Entity>()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Complex_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"Flat_Entity",
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false),
					QueryExpression.DefineColumn("Flat_Id", SqlDataType.Int(), isNullable: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					"Flat_Entity",
					new[] { "Value" },
					new object[] { 5 },
					new object[] { 6 }
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value", "Flat_Id" },
					new object[] { 3, 1 },
					new object[] { 4, 2 }
					));

				var dataSet = new SqlDataSet<Complex_Entity>(sqlDataModel, dataProvider);
				var valueSet = dataSet.Select(q => q.Flat.Value).Select(q => q.Flat.Value).Select(q => q.Value).ToList();

				Assert.IsTrue(
					valueSet.Select(q => q.Item1).SequenceEqual(new[] { 5, 6 })
					);
				Assert.IsTrue(
					valueSet.Select(q => q.Item2).SequenceEqual(new[] { 5, 6 })
					);
				Assert.IsTrue(
					valueSet.Select(q => q.Item3).SequenceEqual(new[] { 3, 4 })
					);
			}
		}

		[TestMethod]
		public void Select_Entity_With_Join_Returns_Entity()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Complex_Entity>()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Complex_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"Flat_Entity",
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false),
					QueryExpression.DefineColumn("Flat_Id", SqlDataType.Int(), isNullable: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					"Flat_Entity",
					new[] { "Value" },
					new object[] { 5 },
					new object[] { 6 }
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value", "Flat_Id" },
					new object[] { 3, 1 },
					new object[] { 4, 2 }
					));

				var dataSet = new SqlDataSet<Complex_Entity>(sqlDataModel, dataProvider);
				var entitySet = dataSet.Select().ToList();

				Assert.IsTrue(
					entitySet.Select(q => q.Value).SequenceEqual(new[] { 3, 4 })
					);
				Assert.IsTrue(
					entitySet.Select(q => q.Flat.Id).SequenceEqual(new[] { 1, 2 })
					);
				Assert.IsTrue(
					entitySet.Select(q => q.Flat.Value).SequenceEqual(new[] { 5, 6 })
					);
			}
		}

		[TestMethod]
		public void Select_Entity_With_Complex_Join_Returns_Entity()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Really_Complex_Entity>()
				.AddSqlEntity<Complex_Entity>()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Really_Complex_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"Flat_Entity",
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"Complex_Entity",
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false),
					QueryExpression.DefineColumn("Flat_Id", SqlDataType.Int(), isNullable: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false),
					QueryExpression.DefineColumn("Complex_Id", SqlDataType.Int(), isNullable: false),
					QueryExpression.DefineColumn("Flat_Id", SqlDataType.Int(), isNullable: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					"Flat_Entity",
					new[] { "Value" },
					new object[] { 7 },
					new object[] { 8 }
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					"Complex_Entity",
					new[] { "Value", "Flat_Id" },
					new object[] { 6, 1 }
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value", "Flat_Id", "Complex_Id" },
					new object[] { 5, 2, 1 }
					));

				var dataSet = new SqlDataSet<Really_Complex_Entity>(sqlDataModel, dataProvider);
				var entitySet = dataSet.Select().ToList();
				var entity = entitySet[0];

				Assert.AreEqual(1, entity.Id);
				Assert.AreEqual(5, entity.Value);
				Assert.AreEqual(2, entity.Flat.Id);
				Assert.AreEqual(8, entity.Flat.Value);
				Assert.AreEqual(1, entity.Complex.Id);
				Assert.AreEqual(6, entity.Complex.Value);
				Assert.AreEqual(1, entity.Complex.Flat.Id);
				Assert.AreEqual(7, entity.Complex.Flat.Value);
			}
		}

		[TestMethod]
		public void Select_Expression_With_Complex_Join_Returns_Value()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Really_Complex_Entity>()
				.AddSqlEntity<Complex_Entity>()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Really_Complex_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"Flat_Entity",
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"Complex_Entity",
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false),
					QueryExpression.DefineColumn("Flat_Id", SqlDataType.Int(), isNullable: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false),
					QueryExpression.DefineColumn("Complex_Id", SqlDataType.Int(), isNullable: false),
					QueryExpression.DefineColumn("Flat_Id", SqlDataType.Int(), isNullable: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					"Flat_Entity",
					new[] { "Value" },
					new object[] { 7 },
					new object[] { 8 }
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					"Complex_Entity",
					new[] { "Value", "Flat_Id" },
					new object[] { 6, 1 }
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value", "Flat_Id", "Complex_Id" },
					new object[] { 5, 2, 1 }
					));

				var dataSet = new SqlDataSet<Really_Complex_Entity>(sqlDataModel, dataProvider);
				var valueSet = dataSet.Select(q => q.Complex.Flat.Value).ToList();

				Assert.AreEqual(7, valueSet[0]);
			}
		}

		[TestMethod]
		public void Select_ComplexExpression_Returns_Value()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value" },
					new object[] { 1 },
					new object[] { 2 }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				var valueSet = dataSet.Select(q => q.Value * 2).ToList();

				Assert.IsTrue(
					valueSet.SequenceEqual(new[] { 2, 4 })
					);
			}
		}

		[TestMethod]
		public void Select_Expression_Returns_Value_Non_Default_Column_Name()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>(cfg => cfg.Field(q => q.Value, field => field.ColumnName("Custom")))
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Custom", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Custom" },
					new object[] { 1 },
					new object[] { 2 }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				var valueSet = dataSet.Select(q => q.Value).ToList();

				Assert.IsTrue(
					valueSet.SequenceEqual(new[] { 1, 2 })
					);
			}
		}

		[TestMethod]
		public void Select_Expression_Returns_Value_Non_Default_Table_Name()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"CustomTableName",
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					"CustomTableName",
					new[] { "Value" },
					new object[] { 1 },
					new object[] { 2 }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				var valueSet = dataSet.Select(q => q.Value).Table("CustomTableName").ToList();

				Assert.IsTrue(
					valueSet.SequenceEqual(new[] { 1, 2 })
					);
			}
		}

		[TestMethod]
		public void Ordered_Select_Returns_Sorted_Values()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value" },
					new object[] { 2 },
					new object[] { 1 },
					new object[] { 3 }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				var valueSet = dataSet.Select(q => q.Value).OrderBy(q => q.Value).ToList();

				Assert.IsTrue(
					valueSet.SequenceEqual(new[] { 1, 2, 3 })
					);
			}
		}

		[TestMethod]
		public void Ordered_Descending_Select_Returns_Sorted_Values()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value" },
					new object[] { 2 },
					new object[] { 1 },
					new object[] { 3 }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				var valueSet = dataSet.Select(q => q.Value).OrderByDescending(q => q.Value).ToList();

				Assert.IsTrue(
					valueSet.SequenceEqual(new[] { 3, 2, 1 })
					);
			}
		}

		[TestMethod]
		public void Limited_Select_Returns_ValueSet()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
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
				var valueSet = dataSet.Select(q => q.Value).Limit(2).ToList();

				Assert.IsTrue(
					valueSet.SequenceEqual(new[] { 1, 2 })
					);
			}
		}

		[TestMethod]
		public void Offset_Select_Returns_ValueSet()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
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
				var valueSet = dataSet.Select(q => q.Value).Offset(1).ToList();

				Assert.IsTrue(
					valueSet.SequenceEqual(new[] { 2, 3 })
					);
			}
		}

		[TestMethod]
		public void Limited_Offset_Select_Returns_ValueSet()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
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
				var valueSet = dataSet.Select(q => q.Value).Limit(1).Offset(1).ToList();

				Assert.IsTrue(
					valueSet.SequenceEqual(new[] { 2 })
					);
			}
		}

		[TestMethod]
		public void Select_Where_Returns_ValueSet()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
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
				var valueSet = dataSet.Select(q => q.Value).AndWhere(q => q.Value == 1).OrWhere(q => q.Value == 2).ToList();

				Assert.IsTrue(
					valueSet.SequenceEqual(new[] { 1, 2 })
					);
			}
		}

		[TestMethod]
		public void Select_Having_Returns_ValueSet()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
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
				var valueSet = dataSet.Select(q => q.Value).GroupBy(q => q.Value).AndHaving(q => q.Value == 1).OrHaving(q => q.Value == 2).ToList();

				Assert.IsTrue(
					valueSet.SequenceEqual(new[] { 1, 2 })
					);
			}
		}

		[TestMethod]
		public void Select_GroupBy_Returns_AggregatedValueSet()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Value", SqlDataType.Int(), isNullable: false, isAutoIncrement: false, isPrimaryKey: false)
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Value" },
					new object[] { 1 },
					new object[] { 1 },
					new object[] { 2 },
					new object[] { 2 },
					new object[] { 3 },
					new object[] { 3 }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				var valueSet = dataSet.Select(q => q.Value).GroupBy(q => q.Value).ToList();

				Assert.IsTrue(
					valueSet.SequenceEqual(new[] { 1, 2, 3 })
					);
			}
		}

		[TestMethod]
		public void Select_Entity_Returns_Entity()
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
				var valueSet = dataSet.Select().ToList();

				Assert.IsTrue(
					valueSet.Select(q => q.Value).SequenceEqual(new[] { 1, 2 })
					);
			}
		}

		[TestMethod]
		public void Select_Single_Entity_Returns_Entity()
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
				var entity = dataSet.Select().ToSingle();

				Assert.AreEqual(1, entity.Value);
			}
		}

		[TestMethod]
		public void Select_Inject_Injects_Values()
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
				var entity = new Flat_Entity();
				dataSet.Select().Inject(entity);

				Assert.AreEqual(1, entity.Id);
				Assert.AreEqual(1, entity.Value);
			}
		}

		[TestMethod]
		public void Select_View_Returns_View()
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
				var valueSet = dataSet.Select<Flat_Entity_Value_View>().ToList();

				Assert.IsTrue(
					valueSet.Select(q => q.Value).SequenceEqual(new[] { 1, 2 })
					);
			}
		}

		[TestMethod]
		public void Select_View_And_Expression_Returns_Zipped_DataSet()
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
					new object[] { 2 }, // id = 1
					new object[] { 4 }  // id = 2
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				var valueSet = dataSet
					.Select(q => q.Id)
					.Select<Flat_Entity_Value_View>()
					.ToList();

				Assert.IsTrue(
					valueSet.Select(q => q.Item1).SequenceEqual(new[] { 1, 2 })
					);
				Assert.IsTrue(
					valueSet.Select(q => q.Item2.Value).SequenceEqual(new[] { 2, 4 })
					);
			}
		}

		[TestMethod]
		public void Can_Select_Bound_Complex_Type_With_Transform()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Has_Complex_Type>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Has_Complex_Type>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Complex_A", SqlDataType.Int()),
					QueryExpression.DefineColumn("Complex_B", SqlDataType.Int())
					));

				var a = 2;
				var b = 4;

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "Complex_A", "Complex_B" },
					new object[] { a, b }
					));

				var binding = new TypeBindingBuilder<Has_Complex_Type, Custom_View>()
					.Bind(q => q.Sum, q => q.Complex, o => o.A + o.B)
					.Bind(q => q.Product, q => q.Complex, o => o.A * o.B)
					.BuildBinding();

				var dataSet = new SqlDataSet<Has_Complex_Type>(sqlDataModel, dataProvider);
				var result = dataSet.Select<Custom_View>(binding).ToSingle();

				Assert.AreEqual(a + b, result.Sum);
				Assert.AreEqual(a * b, result.Product);
			}
		}

		[TestMethod]
		public void Does_Not_Select_Computed_Field_On_Entity()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Entity_With_Computed_Field>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Entity_With_Computed_Field>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				//  leave "C" off the schema, if we try to query for "C" it'll throw an exception and fail the test
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("A", SqlDataType.Int()),
					QueryExpression.DefineColumn("B", SqlDataType.Int())
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "A", "B" },
					new object[] { 1, 1 }
					));

				var dataSet = new SqlDataSet<Entity_With_Computed_Field>(sqlDataModel, dataProvider);
				var result = dataSet.Select().ToSingle();
			}
		}

		[TestMethod]
		public void Can_Select_Computed_Field_With_Expression()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Entity_With_Computed_Field>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Entity_With_Computed_Field>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					sqlDataModel.StorageModel.DefaultTableName,
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("A", SqlDataType.Int()),
					QueryExpression.DefineColumn("B", SqlDataType.Int()),
					QueryExpression.DefineColumn("C", SqlDataType.Int())
					));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					sqlDataModel.StorageModel.DefaultTableName,
					new[] { "A", "B", "C" },
					new object[] { 1, 1, 3 } //  note C isn't A + B as defined on the entity class itself to prove it's read direct from the db
					));

				var dataSet = new SqlDataSet<Entity_With_Computed_Field>(sqlDataModel, dataProvider);
				var value = dataSet.Select(q => q.C).ToSingle();
				Assert.AreEqual(3, value);
			}
		}

		private class Flat_Entity
		{
			public int Id { get; private set; }

			public int Value { get; set; }
		}

		private class Flat_Entity_Value_View
		{
			public int Value { get; set; }
		}

		private class Flat_Entity_Id_View
		{
			public int Id { get; set; }
		}

		private class Complex_Entity
		{
			public int Id { get; set; }

			public int Value { get; set; }

			public Flat_Entity Flat { get; set; }
		}

		private class Really_Complex_Entity
		{
			public int Id { get; set; }

			public int Value { get; set; }

			public Complex_Entity Complex { get; set; }

			public Flat_Entity Flat { get; set; }
		}

		private class Has_Complex_Type
		{
			public Complex_Type Complex { get; set; }
		}

		private class Complex_Type
		{
			public int A { get; set; }
			public int B { get; set; }
		}

		private class Custom_View
		{
			public int Sum { get; set; }
			public int Product { get; set; }
		}

		private class Entity_With_Computed_Field
		{
			public int Id { get; private set; }
			public int A { get; set; }
			public int B { get; set; }
			public int C => A + B;
		}
	}
}
