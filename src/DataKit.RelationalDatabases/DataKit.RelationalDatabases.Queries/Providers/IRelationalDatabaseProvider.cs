namespace DataKit.RelationalDatabases.Providers;

/// <summary>
/// Executes queries on a relational database.
/// </summary>
public interface IRelationalDatabaseProvider : IDisposable, IAsyncDisposable
{
    IEnumerable<RowReader> Execute(SqlQuery sqlQuery);

    IAsyncEnumerable<RowReader> ExecuteAsync(SqlQuery sqlQuery);

    ProviderCommand CreateCommand(SqlQuery sqlQuery);

    Task<ProviderCommand> CreateCommandAsync(SqlQuery sqlQuery, CancellationToken cancellationToken);
}