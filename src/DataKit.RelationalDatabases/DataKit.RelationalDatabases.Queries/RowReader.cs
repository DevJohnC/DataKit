namespace DataKit.RelationalDatabases;

/// <summary>
/// A row of data from a relational database.
/// </summary>
public abstract class RowReader : IDisposable, IAsyncDisposable
{
    public abstract bool Read();

    public abstract Task<bool> ReadAsync(CancellationToken cancellationToken);
    
    public abstract void Dispose();
    public abstract ValueTask DisposeAsync();
}