using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataKit.ORM.Schema;
using DataKit.SQL.QueryExpressions;

namespace DataKit.ORM.UnitTests
{
	[TestClass]
	public class DataContextTests
	{
		[TestMethod]
		public void Create_Sql_Schema_From_DataContext()
		{
			lock (TestDataContext.Lock)
			{
				using (var dataProvider = DataProvider.CreateTestProvider())
				{
					TestDataContext.ConfigureEntity = false;
					var context = DataContext.Create<TestDataContext>(dataProvider);
					Assert.IsNotNull(context.Entities);
				}
			}
		}

		[TestMethod]
		public void Create_Context_Calls_ConfigureSchema_Method()
		{
			lock (TestDataContext.Lock)
			{
				using (var dataProvider = DataProvider.CreateTestProvider())
				{
					TestDataContext.ConfigureInvokeCount = 0;
					TestDataContext.ConfigureEntity = false;
					var context = DataContext.Create<TestDataContext>(dataProvider);
					Assert.AreEqual(1, TestDataContext.ConfigureInvokeCount);
				}
			}
		}

		[TestMethod]
		public void ConfigureSchema_Method_Can_Configure_Entity()
		{
			lock (TestDataContext.Lock)
			{
				using (var dataProvider = DataProvider.CreateTestProvider())
				{
					TestDataContext.ConfigureEntity = true;
					var context = DataContext.Create<TestDataContext>(dataProvider);
					Assert.AreEqual("ChangedTableName", context.Entities.DataModel.StorageModel.DefaultTableName);
				}
			}
		}

		[TestMethod]
		public void DataContext_Operates_On_DataSets()
		{
			lock (TestDataContext.Lock)
			{
				using (var dataProvider = DataProvider.CreateTestProvider())
				{
					dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE [EntityType]
					(
						[Id] INTEGER PRIMARY KEY AUTOINCREMENT,
						[Value] INTEGER
					)"));

					TestDataContext.ConfigureEntity = false;
					var context = DataContext.Create<TestDataContext>(dataProvider);

					var testEntityInstance = new EntityType
					{
						Value = 1
					};
					context.Entities.Insert(testEntityInstance)
						.Execute();

					var retrievedInstance = context.Entities.Select()
						.AndWhere(q => q.Value == testEntityInstance.Value)
						.ToSingle();

					Assert.AreEqual(testEntityInstance.Value, retrievedInstance.Value);
				}
			}
		}

		[TestMethod]
		public void DataContext_Transaction_Rollsback_On_Dispose()
		{
			lock (TestDataContext.Lock)
			{
				using (var dataProvider = DataProvider.CreateTestProvider())
				{
					dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE [EntityType]
					(
						[Id] INTEGER PRIMARY KEY AUTOINCREMENT,
						[Value] INTEGER
					)"));

					TestDataContext.ConfigureEntity = false;
					var context = DataContext.Create<TestDataContext>(dataProvider);

					var testEntityInstance = new EntityType
					{
						Value = 1
					};

					using (var transaction = context.CreateTransaction())
					{
						context.Entities.Insert(testEntityInstance)
							.Execute();

						var retrievedInstances = context.Entities.Select()
							.ToList();

						Assert.AreEqual(1, retrievedInstances.Count);
					}

					var outerRetrievedInstances = context.Entities.Select()
						.ToList();

					Assert.AreEqual(0, outerRetrievedInstances.Count);
				}
			}
		}

		[TestMethod]
		public void DataContext_Transaction_Commits()
		{
			lock (TestDataContext.Lock)
			{
				using (var dataProvider = DataProvider.CreateTestProvider())
				{
					dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE [EntityType]
					(
						[Id] INTEGER PRIMARY KEY AUTOINCREMENT,
						[Value] INTEGER
					)"));

					TestDataContext.ConfigureEntity = false;
					var context = DataContext.Create<TestDataContext>(dataProvider);

					var testEntityInstance = new EntityType
					{
						Value = 1
					};

					using (var transaction = context.CreateTransaction())
					{
						context.Entities.Insert(testEntityInstance)
							.Execute();

						var retrievedInstances = context.Entities.Select()
							.ToList();

						Assert.AreEqual(1, retrievedInstances.Count);

						transaction.Commit();
					}

					var outerRetrievedInstances = context.Entities.Select()
						.ToList();

					Assert.AreEqual(1, outerRetrievedInstances.Count);
				}
			}
		}

		private class TestDataContext : DataContext
		{
			public static readonly object Lock = new object();

			public static int ConfigureInvokeCount = 0;

			public static bool ConfigureEntity = false;

			public SqlDataSet<EntityType> Entities { get; private set; }

			public static void ConfigureSchema(DataSchemaBuilder builder)
			{
				ConfigureInvokeCount++;

				if (ConfigureEntity)
				{
					builder.ConfigureSqlEntity<EntityType>(config =>
					{
						config.DefaultTableName = "ChangedTableName";
					});
				}
			}
		}

		private class EntityType
		{
			public int Id { get; protected set; }

			public int Value { get; set; }
		}
	}
}
