using DataKit.RelationalDatabases.Providers;
using DataKit.RelationalDatabases.QueryExpressions;
using static DataKit.RelationalDatabases.QueryExpressions.QueryExpression;

namespace DataKit.RelationalDatabases.Queries.UnitTests;

[TestClass]
public sealed class RawSqlQueryTests
{
    [TestMethod]
    public void Can_Enumerate_Rows()
    {
        using var provider = SqliteRelationalDatabaseProvider.CreateInMemory();

        var rowCount = 0;
        foreach (var row in provider.Execute(new SqlQuery("SELECT 1")))
        {
            rowCount++;
            Assert.AreEqual(1, row.ReadInt32(0));
        }

        Assert.AreEqual(1, rowCount);
    }
    
    [TestMethod]
    public void Can_Enumerate_QueryExpression_Rows()
    {
        using IRelationalDatabaseProvider provider = SqliteRelationalDatabaseProvider.CreateInMemory();

        var rowCount = 0;
        foreach (var row in provider.Execute(Select(Constant(1))))
        {
            rowCount++;
            Assert.AreEqual(1, row.ReadInt32(0));
        }

        Assert.AreEqual(1, rowCount);
    }
    
    [TestMethod]
    public async Task Can_Enumerate_Rows_Async()
    {
        await using var provider = SqliteRelationalDatabaseProvider.CreateInMemory();

        var rowCount = 0;
        await foreach (var row in provider.ExecuteAsync(new SqlQuery("SELECT 1")))
        {
            rowCount++;
            Assert.AreEqual(1, row.ReadInt32(0));
        }

        Assert.AreEqual(1, rowCount);
    }
    
    [TestMethod]
    public async Task Can_Enumerate_QueryExpression_Rows_Async()
    {
        await using IRelationalDatabaseProvider provider = SqliteRelationalDatabaseProvider.CreateInMemory();

        var rowCount = 0;
        await foreach (var row in provider.ExecuteAsync(Select(Constant(1))))
        {
            rowCount++;
            Assert.AreEqual(1, row.ReadInt32(0));
        }

        Assert.AreEqual(1, rowCount);
    }
}