using System.Collections;

namespace DataKit.RelationalDatabases.Providers;

public sealed class SyncQuery : IEnumerable<RowReader>
{
    private readonly IRelationalDatabaseProvider _provider;
    private readonly SqlQuery _sqlQuery;

    public SyncQuery(IRelationalDatabaseProvider provider, SqlQuery sqlQuery)
    {
        _provider = provider;
        _sqlQuery = sqlQuery;
    }
    
    public IEnumerator<RowReader> GetEnumerator()
    {
        ProviderCommand? command = null;
        RowReader? reader = null;
        try
        {
            command = _provider.CreateCommand(_sqlQuery);
            reader = command.ExecuteReader();
            return new SyncRowReader(command, reader);
        }
        catch
        {
            command?.Dispose();
            reader?.Dispose();
            throw;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}