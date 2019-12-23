using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataKit.ORM.Schema;
using DataKit.ORM.Schema.Sql;
using System.Linq;
using DataKit.SQL.QueryExpressions;

namespace DataKit.ORM.UnitTests
{
	[TestClass]
	public class DataSetInsertTests
	{
		[TestMethod]
		public void Insert_Returns_Number_Of_Inserted_Rows()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE {sqlDataModel.StorageModel.DefaultTableName}
					(
						[Value] INTEGER
					)"));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);

				var value = 5;
				Assert.AreEqual(1, dataSet.Insert(new Flat_Entity { Value = value }).Execute());
			}
		}

		[TestMethod]
		public void Insert_Default_Values()
		{
			var defaultValue = 4;
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>(cfg => cfg.Field(q => q.Value, cfg => cfg.DefaultValue(() => defaultValue)))
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE {sqlDataModel.StorageModel.DefaultTableName}
					(
						[Value] INTEGER
					)"));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);

				dataSet.Insert().Execute();

				using (var result = dataProvider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, from: QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName))))
				{
					Assert.IsTrue(result.HasRows);
					Assert.IsTrue(result.Read());
					Assert.AreEqual(defaultValue, result.GetInt32(0));
				}
			}
		}

		[TestMethod]
		public void Insert_Custom_Flat_ColumnName()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>(cfg => cfg.Field(q => q.Value, cfg => cfg.ColumnName("Custom")))
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE {sqlDataModel.StorageModel.DefaultTableName}
					(
						[Custom] INTEGER
					)"));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);

				var entity = new Flat_Entity { Value = 5 };
				dataSet.Insert(entity).Execute();

				using (var result = dataProvider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, from: QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName))))
				{
					Assert.IsTrue(result.HasRows);
					Assert.IsTrue(result.Read());
					Assert.AreEqual(entity.Value, result.GetInt32(0));
				}
			}
		}

		[TestMethod]
		public void Insert_Custom_Embedded_ColumnName()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Complex_Entity>(cfg => cfg.Field(q => q.Flat.Value, cfg => cfg.ColumnName("Custom")).AutoModel())
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Complex_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE {sqlDataModel.StorageModel.DefaultTableName}
					(
						[Value] INTEGER,
						[Custom] INTEGER,
						[Flat_Id] INTEGER
					)"));

				var dataSet = new SqlDataSet<Complex_Entity>(sqlDataModel, dataProvider);

				var entity = new Complex_Entity { Value = 5, Flat = new Flat_Entity { Value = 6 } };
				dataSet.Insert(entity).Execute();

				using (var result = dataProvider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, from: QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName))))
				{
					Assert.IsTrue(result.HasRows);
					Assert.IsTrue(result.Read());
					Assert.AreEqual(entity.Value, result.GetInt32(0));
					Assert.AreEqual(entity.Flat.Value, result.GetInt32(1));
				}
			}
		}

		[TestMethod]
		public void Insert_ExpressionField_Value()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE {sqlDataModel.StorageModel.DefaultTableName}
					(
						[Value] INTEGER
					)"));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);

				var value = 5;
				dataSet.Insert()
					.Set(q => q.Value, value)
					.Execute();

				using (var result = dataProvider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, from: QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName))))
				{
					Assert.IsTrue(result.HasRows);
					Assert.IsTrue(result.Read());
					Assert.AreEqual(value, result.GetInt32(0));
				}
			}
		}

		[TestMethod]
		public void Insert_ExpressionField_Value_Overrides_Default()
		{
			var defaultValue = 4;
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>(cfg => cfg.Field(q => q.Value, cfg => cfg.DefaultValue(() => defaultValue)))
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE {sqlDataModel.StorageModel.DefaultTableName}
					(
						[Value] INTEGER
					)"));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);

				var value = 5;
				dataSet.Insert()
					.Set(q => q.Value, value)
					.Execute();

				using (var result = dataProvider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, from: QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName))))
				{
					Assert.IsTrue(result.HasRows);
					Assert.IsTrue(result.Read());
					var readValue = result.GetInt32(0);
					Assert.AreNotEqual(defaultValue, readValue);
					Assert.AreEqual(value, readValue);
				}
			}
		}

		[TestMethod]
		public void Insert_Flat_Entity()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE {sqlDataModel.StorageModel.DefaultTableName}
					(
						[Value] INTEGER
					)"));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);

				var value = 5;
				dataSet.Insert(new Flat_Entity { Value = value }).Execute();

				using (var result = dataProvider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, from: QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName))))
				{
					Assert.IsTrue(result.HasRows);
					Assert.IsTrue(result.Read());
					Assert.AreEqual(value, result.GetInt32(0));
				}
			}
		}

		[TestMethod]
		public void Insert_Flat_Entity_Into_NonDefault_Table()
		{
			var tableName = "MyCustomTable";
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE MyCustomTable
					(
						[Value] INTEGER
					)"));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);

				var value = 5;
				dataSet.Insert(new Flat_Entity { Value = value }).Table(tableName).Execute();

				using (var result = dataProvider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, from: QueryExpression.Table(tableName))))
				{
					Assert.IsTrue(result.HasRows);
					Assert.IsTrue(result.Read());
					Assert.AreEqual(value, result.GetInt32(0));
				}
			}
		}

		[TestMethod]
		public void Insert_Flat_View_Without_Binding()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE {sqlDataModel.StorageModel.DefaultTableName}
					(
						[Value] INTEGER
					)"));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);

				var value = 5;
				dataSet.Insert(new { Value = value }).Execute();

				using (var result = dataProvider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, from: QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName))))
				{
					Assert.IsTrue(result.HasRows);
					Assert.IsTrue(result.Read());
					Assert.AreEqual(value, result.GetInt32(0));
				}
			}
		}

		[TestMethod]
		public void Insert_Flat_View_With_Binding()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE {sqlDataModel.StorageModel.DefaultTableName}
					(
						[Value] INTEGER
					)"));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				var value = 5;
				var view = new { Custom = 5 };

				dataSet.Insert(view, binding => binding.Bind(t => t.Value, s => s.Custom)).Execute();

				using (var result = dataProvider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, from: QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName))))
				{
					Assert.IsTrue(result.HasRows);
					Assert.IsTrue(result.Read());
					Assert.AreEqual(value, result.GetInt32(0));
				}
			}
		}

		[TestMethod]
		public void Insert_Flat_View_With_Binding_And_Transform()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE {sqlDataModel.StorageModel.DefaultTableName}
					(
						[Value] INTEGER
					)"));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				var value = 5;
				var view = new { Custom = value.ToString() };

				var transformWasCalled = false;
				dataSet.Insert(view, binding =>
					binding.Bind(t => t.Value, s => s.Custom, value =>
					{
						transformWasCalled = true;
						return int.Parse(value);
					})).Execute();

				Assert.IsTrue(transformWasCalled);
				using (var result = dataProvider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, from: QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName))))
				{
					Assert.IsTrue(result.HasRows);
					Assert.IsTrue(result.Read());
					Assert.AreEqual(value, result.GetInt32(0));
				}
			}
		}

		[TestMethod]
		public void Can_Insert_Entity_With_Null_Member()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Complex_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Complex_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE {sqlDataModel.StorageModel.DefaultTableName}
					(
						[Value] INTEGER,
						[Flat_Value] INTEGER,
						[Flat_Id] INTEGER
					)"));

				var dataSet = new SqlDataSet<Complex_Entity>(sqlDataModel, dataProvider);

				var entity = new Complex_Entity
				{
					Value = 5
				};
				dataSet.Insert(entity).Execute();

				using (var result = dataProvider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, from: QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName))))
				{
					Assert.IsTrue(result.HasRows);
					Assert.IsTrue(result.Read());
					Assert.AreEqual(entity.Value, result.GetInt32(0));
					Assert.AreEqual(0, result.GetInt32(1));
				}
			}
		}

		[TestMethod]
		public void Insert_Complex_Embedded_Entity()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Complex_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Complex_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE {sqlDataModel.StorageModel.DefaultTableName}
					(
						[Value] INTEGER,
						[Flat_Value] INTEGER,
						[Flat_Id] INTEGER
					)"));

				var dataSet = new SqlDataSet<Complex_Entity>(sqlDataModel, dataProvider);

				var entity = new Complex_Entity
				{
					Value = 5,
					Flat = new Flat_Entity
					{
						Value = 6
					}
				};
				dataSet.Insert(entity).Execute();

				using (var result = dataProvider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, from: QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName))))
				{
					Assert.IsTrue(result.HasRows);
					Assert.IsTrue(result.Read());
					Assert.AreEqual(entity.Value, result.GetInt32(0));
					Assert.AreEqual(entity.Flat.Value, result.GetInt32(1));
				}
			}
		}

		[TestMethod]
		public void Insert_Complex_Embedded_View()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Complex_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Complex_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE {sqlDataModel.StorageModel.DefaultTableName}
					(
						[Value] INTEGER,
						[Flat_Value] INTEGER,
						[Flat_Id] INTEGER
					)"));

				var dataSet = new SqlDataSet<Complex_Entity>(sqlDataModel, dataProvider);

				var view = new
				{
					Value = 5,
					Flat = new
					{
						Value = 6
					}
				};
				dataSet.Insert(view).Execute();

				using (var result = dataProvider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, from: QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName))))
				{
					Assert.IsTrue(result.HasRows);
					Assert.IsTrue(result.Read());
					Assert.AreEqual(view.Value, result.GetInt32(0));
					Assert.AreEqual(view.Flat.Value, result.GetInt32(1));
				}
			}
		}

		[TestMethod]
		public void Insert_Complex_Embedded_FlatView()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Complex_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Complex_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE {sqlDataModel.StorageModel.DefaultTableName}
					(
						[Value] INTEGER,
						[Flat_Value] INTEGER,
						[Flat_Id] INTEGER
					)"));

				var dataSet = new SqlDataSet<Complex_Entity>(sqlDataModel, dataProvider);

				var view = new
				{
					Value = 5,
					FlatValue = 6
				};
				dataSet.Insert(view).Execute();

				using (var result = dataProvider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, from: QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName))))
				{
					Assert.IsTrue(result.HasRows);
					Assert.IsTrue(result.Read());
					Assert.AreEqual(view.Value, result.GetInt32(0));
					Assert.AreEqual(view.FlatValue, result.GetInt32(1));
				}
			}
		}

		[TestMethod]
		public void Insert_Complex_Embedded_View_With_Custom_Binding()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Complex_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Complex_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE {sqlDataModel.StorageModel.DefaultTableName}
					(
						[Value] INTEGER,
						[Flat_Value] INTEGER,
						[Flat_Id] INTEGER
					)"));

				var dataSet = new SqlDataSet<Complex_Entity>(sqlDataModel, dataProvider);

				var view = new
				{
					TopValue = 5,
					DeepValue = 6
				};
				dataSet.Insert(view, binding =>
				{
					binding
						.Bind(t => t.Value, s => s.TopValue)
						.Bind(t => t.Flat.Value, s => s.DeepValue);
				}).Execute();

				using (var result = dataProvider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, from: QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName))))
				{
					Assert.IsTrue(result.HasRows);
					Assert.IsTrue(result.Read());
					Assert.AreEqual(view.TopValue, result.GetInt32(0));
					Assert.AreEqual(view.DeepValue, result.GetInt32(1));
				}
			}
		}

		[TestMethod]
		public void Insert_Complex_Embedded_View_With_Complex_Type_Binding()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Complex_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Complex_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE {sqlDataModel.StorageModel.DefaultTableName}
					(
						[Value] INTEGER,
						[Flat_Value] INTEGER,
						[Flat_Id] INTEGER
					)"));

				var dataSet = new SqlDataSet<Complex_Entity>(sqlDataModel, dataProvider);

				var view = new
				{
					TopValue = 5,
					DeepValue = 6
				};
				dataSet.Insert(view, binding =>
				{
					binding
						.Bind(t => t.Value, s => s.TopValue)
						.Bind(t => t.Flat, s => s.DeepValue, value => new Flat_Entity { Value = value });
				}).Execute();

				using (var result = dataProvider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, from: QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName))))
				{
					Assert.IsTrue(result.HasRows);
					Assert.IsTrue(result.Read());
					Assert.AreEqual(view.TopValue, result.GetInt32(0));
					Assert.AreEqual(view.DeepValue, result.GetInt32(1));
				}
			}
		}

		[TestMethod]
		public void Insert_Complex_Entity_With_Relationship()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.AddSqlEntity<Complex_Entity>()
				.Build();
			var flatDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();
			var complexDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Complex_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE {flatDataModel.StorageModel.DefaultTableName}
					(
						[Value] INTEGER,
						[Id] INTEGER PRIMARY KEY AUTOINCREMENT
					)"));
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE {complexDataModel.StorageModel.DefaultTableName}
					(
						[Value] INTEGER,
						[Flat_Id] INTEGER
					)"));

				var flatDataSet = new SqlDataSet<Flat_Entity>(flatDataModel, dataProvider);
				var complexDataSet = new SqlDataSet<Complex_Entity>(complexDataModel, dataProvider);

				//  the ID here isn't written to the INSERT query because the ORM see's it as server generated (which it should be)
				var flatEntity = new Flat_Entity { Id = 1, Value = 5 };
				var complexEntity = new Complex_Entity { Value = 6, Flat = flatEntity };

				flatDataSet.Insert(flatEntity).Execute();
				complexDataSet.Insert(complexEntity).Execute();

				using (var result = dataProvider.ExecuteReader(QueryExpression.Select(
					new[] { QueryExpression.All() },
					from: QueryExpression.Table(complexDataModel.StorageModel.DefaultTableName),
					joins: new[]
					{
						QueryExpression.Join(
							QueryExpression.Table(flatDataModel.StorageModel.DefaultTableName),
							QueryExpression.AreEqual(
								QueryExpression.Column("Flat_Id", QueryExpression.Table(complexDataModel.StorageModel.DefaultTableName)),
								QueryExpression.Column("Id", QueryExpression.Table(flatDataModel.StorageModel.DefaultTableName))
								), JoinDirection.Left
							)
					})))
				{
					Assert.IsTrue(result.HasRows);
					Assert.IsTrue(result.Read());
					Assert.AreEqual(complexEntity.Value, result.GetInt32(0));
					Assert.AreEqual(complexEntity.Flat.Id, result.GetInt32(1));
					Assert.AreEqual(flatEntity.Value, result.GetInt32(2));
					Assert.AreEqual(flatEntity.Id, result.GetInt32(3));
				}
			}
		}

		[TestMethod]
		public void Insert_Complex_Entity_With_Relationship_View()
		{
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.AddSqlEntity<Complex_Entity>()
				.Build();
			var flatDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();
			var complexDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Complex_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE {flatDataModel.StorageModel.DefaultTableName}
					(
						[Value] INTEGER,
						[Id] INTEGER PRIMARY KEY AUTOINCREMENT
					)"));
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE {complexDataModel.StorageModel.DefaultTableName}
					(
						[Value] INTEGER,
						[Flat_Id] INTEGER
					)"));

				var flatDataSet = new SqlDataSet<Flat_Entity>(flatDataModel, dataProvider);
				var complexDataSet = new SqlDataSet<Complex_Entity>(complexDataModel, dataProvider);

				//  the ID here isn't written to the INSERT query because the ORM see's it as server generated (which it should be)
				var flatEntity = new Flat_Entity { Id = 1, Value = 5 };
				var complexEntityView = new { Value = 6, FlatId = flatEntity.Id };

				flatDataSet.Insert(flatEntity).Execute();
				complexDataSet.Insert(complexEntityView).Execute();

				using (var result = dataProvider.ExecuteReader(QueryExpression.Select(
					new[] { QueryExpression.All() },
					from: QueryExpression.Table(complexDataModel.StorageModel.DefaultTableName),
					joins: new[]
					{
						QueryExpression.Join(
							QueryExpression.Table(flatDataModel.StorageModel.DefaultTableName),
							QueryExpression.AreEqual(
								QueryExpression.Column("Flat_Id", QueryExpression.Table(complexDataModel.StorageModel.DefaultTableName)),
								QueryExpression.Column("Id", QueryExpression.Table(flatDataModel.StorageModel.DefaultTableName))
								)
							)
					})))
				{
					Assert.IsTrue(result.HasRows);
					Assert.IsTrue(result.Read());
					Assert.AreEqual(complexEntityView.Value, result.GetInt32(0));
					Assert.AreEqual(complexEntityView.FlatId, result.GetInt32(1));
					Assert.AreEqual(flatEntity.Value, result.GetInt32(2));
					Assert.AreEqual(flatEntity.Id, result.GetInt32(3));
				}
			}
		}

		private class Flat_Entity
		{
			public int Id { get; set; }

			public int Value { get; set; }
		}

		private class Complex_Entity
		{
			public int Value { get; set; }

			public Flat_Entity Flat { get; set; }
		}
	}
}
