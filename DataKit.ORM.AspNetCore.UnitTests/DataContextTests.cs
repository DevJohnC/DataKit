using DataKit.Mapping.AspNetCore;
using DataKit.ORM.AspNetCore.Sql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL;
using Silk.Data.SQL.Expressions;

namespace DataKit.ORM.AspNetCore.UnitTests
{
	[TestClass]
	public class DataContextTests
	{
		[TestMethod]
		public void Can_Create_DataContext_From_Container()
		{
			using (var dataProvider = TestHelpers.CreateTestProvider())
			{
				var container = TestHelpers.CreateContainer(services =>
				{
					services.AddDataContext<TestContext>(dataProvider);
				});

				using (var scope = container.CreateScope())
				{
					var dataContext = scope.ServiceProvider.GetRequiredService<TestContext>();
					Assert.IsNotNull(dataContext);
					Assert.IsInstanceOfType(dataContext.Entities, typeof(AspNetCoreSqlDataSet<TestEntity>));
				}
			}
		}

		[TestMethod]
		public void Can_Select_With_Custom_Binding()
		{
			using (var dataProvider = TestHelpers.CreateTestProvider())
			{
				var container = TestHelpers.CreateContainer(services =>
				{
					services.AddDataContext<TestContext>(dataProvider);
					services.Configure<MappingOptions>(options =>
					{
						options.AddBindingOverride<TestEntity, TestView>(binding =>
						{
							binding.Bind(t => t.TheId, s => s.Id);
							binding.Bind(t => t.TheValue, s => s.Value, value => value * 2);
						});
					});
				});

				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"TestEntity",
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("Value", SqlDataType.Int())
					));

				using (var scope = container.CreateScope())
				{
					var dataContext = scope.ServiceProvider.GetRequiredService<TestContext>();
					var entity = new TestEntity
					{
						Value = 2
					};
					Batch.Create()
						.Insert(dataContext.Entities.Insert(entity))
						.Inject(dataContext.Entities.Select().LastInsertId(), entity)
						.Single(dataContext.Entities.Select<TestView>(), out var fetchResult)
						.Execute();
					var fetchedEntity = fetchResult.Result;

					Assert.AreEqual(entity.Id, fetchedEntity.TheId);
					Assert.AreEqual(entity.Value * 2, fetchedEntity.TheValue);
				}
			}
		}

		[TestMethod]
		public void Can_Insert_With_Custom_Binding()
		{
			using (var dataProvider = TestHelpers.CreateTestProvider())
			{
				var container = TestHelpers.CreateContainer(services =>
				{
					services.AddDataContext<TestContext>(dataProvider);
					services.Configure<MappingOptions>(options =>
					{
						options.AddBindingOverride<TestView, TestEntity>(binding =>
						{
							binding.Bind(t => t.Id, s => s.TheId);
							binding.Bind(t => t.Value, s => s.TheValue, value => value / 2);
						});
					});
				});

				dataProvider.ExecuteNonQuery(QueryExpression.CreateTable(
					"TestEntity",
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("Value", SqlDataType.Int())
					));

				using (var scope = container.CreateScope())
				{
					var dataContext = scope.ServiceProvider.GetRequiredService<TestContext>();
					var entityView = new TestView
					{
						TheValue = 4
					};
					Batch.Create()
						.Insert(dataContext.Entities.Insert(entityView))
						.Single(dataContext.Entities.Select(), out var fetchResult)
						.Execute();
					var fetchedEntity = fetchResult.Result;

					Assert.AreEqual(1, fetchedEntity.Id);
					Assert.AreEqual(entityView.TheValue / 2, fetchedEntity.Value);
				}
			}
		}

		private class TestContext : DataContext
		{
			public SqlDataSet<TestEntity> Entities { get; private set; }
		}

		private class TestEntity
		{
			public int Id { get; private set; }
			public int Value { get; set; }
		}

		private class TestView
		{
			public int TheId { get; private set; }
			public int TheValue { get; set; }
		}
	}
}
