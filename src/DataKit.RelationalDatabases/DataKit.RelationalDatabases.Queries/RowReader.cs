using System.Data.Common;

namespace DataKit.RelationalDatabases;

/// <summary>
/// A row of data from a relational database.
/// </summary>
public abstract class RowReader : IDisposable, IAsyncDisposable
{
    private readonly DbDataReader _reader;

    protected RowReader(DbDataReader reader)
    {
        _reader = reader;
    }

    public virtual int ReadInt32(int ord)
    {
        return _reader.GetInt32(ord);
    }

    public virtual bool Read() => _reader.Read();

    public virtual Task<bool> ReadAsync(CancellationToken cancellationToken) => _reader.ReadAsync(cancellationToken);

    public virtual void Dispose() => _reader.Dispose();

    public virtual ValueTask DisposeAsync() => _reader.DisposeAsync();
}