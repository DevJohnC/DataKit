using DataKit.SQL.QueryExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DataKit.ORM.UnitTests
{
	[TestClass]
	public class BatchTests
	{
		[TestMethod]
		public void Batch_Sql_Operates()
		{
			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE Entities
					(
						[Id] INTEGER PRIMARY KEY AUTOINCREMENT,
						[Value] INTEGER
					)"));

				var context = DataContext.Create<TestDataContext>(dataProvider);
				var newEntityOne = new EntityType { Value = 2 };
				var newEntityTwo = new EntityType { Value = 3 };
				var retrievedEntity = new EntityType();

				Batch.Create()
					.Insert(context.Entities.Insert(newEntityOne))
					.Insert(context.Entities.Insert(newEntityTwo))
					.Update(context.Entities.Update().Set(q => q.Value, q => q.Value + 1))
					.Inject(context.Entities.Select().AndWhere(q => q.Id == 2), retrievedEntity)
					.Delete(context.Entities.Delete().AndWhere(q => q.Id == 1))
					.List(context.Entities.Select(q => q.Value), out var fullValueSetResult)
					.Single(context.Entities.Select(q => q.Id), out var singleIdResult)
					.Execute();

				Assert.AreEqual(2, retrievedEntity.Id);
				Assert.AreEqual(4, retrievedEntity.Value);
				Assert.IsTrue(new[] { 4 }.SequenceEqual(fullValueSetResult.Result));
				Assert.AreEqual(2, singleIdResult.Result);
			}
		}

		[TestMethod]
		public void Can_Insert_Relationship_And_Query_Entity()
		{
			using (var dataProvider = DataProvider.CreateTestProvider())
			{
				var context = DataContext.Create<TestDataContext>(dataProvider);

				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE {context.Entities.DataModel.StorageModel.DefaultTableName}
					(
						[Id] INTEGER PRIMARY KEY AUTOINCREMENT,
						[Value] INTEGER
					)"));
				dataProvider.ExecuteNonQuery(Sqlite3QueryExpression.Raw($@"
					CREATE TABLE {context.RelatedEntities.DataModel.StorageModel.DefaultTableName}
					(
						[Id] INTEGER PRIMARY KEY AUTOINCREMENT,
						[Value] INTEGER,
						[Related_Id] INTEGER
					)"));

				var newEntity = new EntityType
				{
					Value = 2
				};
				var relatedEntity = new EntityTypeWithRelationship
				{
					Value = 3
				};

				Batch.Create()
					.Insert(context.Entities.Insert(newEntity))
					.Insert(
						context.RelatedEntities.Insert(relatedEntity)
							.Set(q => q.Related.Id, q => context.SqlServerFunctions.LastInsertId())
						)
					.Single(context.RelatedEntities.Select(), out var entityResult)
					.Execute();

				var entity = entityResult.Result;
				Assert.AreEqual(1, entity.Id);
				Assert.AreEqual(1, entity.Related.Id);
				Assert.AreEqual(newEntity.Value, entity.Related.Value);
				Assert.AreEqual(relatedEntity.Value, entity.Value);
			}
		}

		private class TestDataContext : DataContext
		{
			public SqlDataSet<EntityType> Entities { get; private set; }

			public SqlDataSet<EntityTypeWithRelationship> RelatedEntities { get; private set; }
		}

		private class EntityType
		{
			public int Id { get; protected set; }

			public int Value { get; set; }
		}

		private class EntityTypeWithRelationship
		{
			public int Id { get; protected set; }

			public int Value { get; set; }
			public EntityType Related { get; set; }
		}
	}
}
