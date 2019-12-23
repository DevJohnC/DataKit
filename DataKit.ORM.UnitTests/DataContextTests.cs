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
			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				var context = DataContext.Create<TestDataContext>(dataProvider);
				Assert.IsNotNull(context.Entities);
			}
		}

		[TestMethod]
		public void Create_Context_Calls_ConfigureSchema_Method()
		{
			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				TestDataContext.ConfigureInvokeCount = 0;
				var context = DataContext.Create<TestDataContext>(dataProvider);
				Assert.AreEqual(1, TestDataContext.ConfigureInvokeCount);
			}
		}

		[TestMethod]
		public void DataContext_Operates_On_DataSets()
		{
			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE [EntityType]
					(
						[Id] INTEGER PRIMARY KEY AUTOINCREMENT,
						[Value] INTEGER
					)"));

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

		[TestMethod]
		public void DataContext_Transaction_Rollsback_On_Dispose()
		{
			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE [EntityType]
					(
						[Id] INTEGER PRIMARY KEY AUTOINCREMENT,
						[Value] INTEGER
					)"));

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

		[TestMethod]
		public void DataContext_Transaction_Commits()
		{
			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE [EntityType]
					(
						[Id] INTEGER PRIMARY KEY AUTOINCREMENT,
						[Value] INTEGER
					)"));

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

		private class TestDataContext : DataContext
		{
			public static int ConfigureInvokeCount = 0;

			public SqlDataSet<EntityType> Entities { get; private set; }

			public static void ConfigureSchema(DataSchemaBuilder builder)
			{
				ConfigureInvokeCount++;
			}
		}

		private class EntityType
		{
			public int Id { get; protected set; }

			public int Value { get; set; }
		}
	}
}
