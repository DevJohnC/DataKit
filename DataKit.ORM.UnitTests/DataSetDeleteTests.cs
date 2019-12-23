using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataKit.ORM.Schema;
using DataKit.ORM.Schema.Sql;
using System.Linq;
using DataKit.SQL.QueryExpressions;

namespace DataKit.ORM.UnitTests
{
	[TestClass]
	public class DataSetDeleteTests
	{
		[TestMethod]
		public void Delete_Can_Delete_From_Non_Default_Table()
		{
			var tableName = "Delete_Can_Delete_From_Non_Default_Table";
			var schema = new DataSchemaBuilder()
				.AddSqlEntity<Flat_Entity>()
				.Build();
			var sqlDataModel = schema.Sql.SqlEntities.OfType<SqlDataModel<Flat_Entity>>().First();

			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE {tableName}
					(
						[Id] INTEGER PRIMARY KEY AUTOINCREMENT,
						[Value] INTEGER
					)"));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					QueryExpression.Table(tableName),
					new[] { QueryExpression.Column("Value") },
					new[] { QueryExpression.Value(5) },
					new[] { QueryExpression.Value(6) }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				//  no need to test the results of this operation, if it fails it'll throw an exception
				dataSet.Delete().Table(tableName).Execute();
			}
		}

		[TestMethod]
		public void Delete_Returns_Number_Of_Deleted_Records()
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
						[Id] INTEGER PRIMARY KEY AUTOINCREMENT,
						[Value] INTEGER
					)"));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName),
					new[] { QueryExpression.Column("Value") },
					new[] { QueryExpression.Value(5) },
					new[] { QueryExpression.Value(6) }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				Assert.AreEqual(2, dataSet.Delete().Execute());
			}
		}

		[TestMethod]
		public void Manual_Delete_Operation_Deletes_Records()
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
						[Id] INTEGER PRIMARY KEY AUTOINCREMENT,
						[Value] INTEGER
					)"));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName),
					new[] { QueryExpression.Column("Value") },
					new[] { QueryExpression.Value(5) },
					new[] { QueryExpression.Value(6) }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				dataSet.Delete().AndWhere(q => q.Value == 5).Execute();

				using (var queryResult = dataProvider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.Column(nameof(Flat_Entity.Value)) }, from: QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName))))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(queryResult.Read());
					Assert.AreEqual(6, queryResult.GetInt32(0));
				}
			}
		}

		[TestMethod]
		public void Entity_Delete_Operation_Deletes_Entity_Record()
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
						[Id] INTEGER PRIMARY KEY AUTOINCREMENT,
						[Value] INTEGER
					)"));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName),
					new[] { QueryExpression.Column("Value") },
					new[] { QueryExpression.Value(5) }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				var entity = new Flat_Entity { Id = 1 };
				dataSet.Delete(entity).Execute();

				using (var queryResult = dataProvider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, from: QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName))))
				{
					Assert.IsFalse(queryResult.HasRows);
				}
			}
		}

		[TestMethod]
		public void Entity_Delete_Operation_Deletes_Only_Entity_Record()
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
						[Id] INTEGER PRIMARY KEY AUTOINCREMENT,
						[Value] INTEGER
					)"));

				dataProvider.ExecuteNonQuery(QueryExpression.Insert(
					QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName),
					new[] { QueryExpression.Column("Value") },
					new[] { QueryExpression.Value(5) },
					new[] { QueryExpression.Value(5) }
					));

				var dataSet = new SqlDataSet<Flat_Entity>(sqlDataModel, dataProvider);
				var entity = new Flat_Entity { Id = 1 };
				dataSet.Delete(entity).Execute();

				using (var queryResult = dataProvider.ExecuteReader(QueryExpression.Select(new[] { QueryExpression.All() }, from: QueryExpression.Table(sqlDataModel.StorageModel.DefaultTableName))))
				{
					Assert.IsTrue(queryResult.HasRows);
				}
			}
		}

		private class Flat_Entity
		{
			public int Id { get; set; }

			public int Value { get; set; }
		}
	}
}
