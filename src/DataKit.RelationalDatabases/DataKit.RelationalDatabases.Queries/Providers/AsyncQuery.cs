namespace DataKit.RelationalDatabases.Providers;

public sealed class AsyncQuery : IAsyncEnumerable<RowReader>
{
    private readonly IRelationalDatabaseProvider _provider;
    private readonly SqlQuery _sqlQuery;

    public AsyncQuery(IRelationalDatabaseProvider provider, SqlQuery sqlQuery)
    {
        _provider = provider;
        _sqlQuery = sqlQuery;
    }

    public IAsyncEnumerator<RowReader> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        ProviderCommand? command = null;
        RowReader? reader = null;
        try
        {
            command = _provider.CreateCommand(_sqlQuery);
            reader = command.ExecuteReader();
            return new AsyncRowReader(command, reader, cancellationToken);
        }
        catch
        {
            command?.Dispose();
            reader?.Dispose();
            throw;
        }
    }
}