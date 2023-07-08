namespace DataKit.RelationalDatabases.Providers;

public abstract class ProviderCommand : IDisposable, IAsyncDisposable
{
    public abstract RowReader ExecuteReader();

    public abstract Task<RowReader> ExecuteReaderAsync(CancellationToken cancellationToken);

    public abstract void Dispose();

    public abstract ValueTask DisposeAsync();
}