namespace DataKit.RelationalDatabases.Providers;

/// <summary>
/// Executes queries on a relational database.
/// </summary>
public interface IRelationalDatabaseProvider
{
    IEnumerable<Row> Execute(SqlQuery sqlQuery);

    IAsyncEnumerable<Row> ExecuteAsync(SqlQuery sqlQuery);
}