using Microsoft.Data.Sqlite;

namespace DataKit.RelationalDatabases.Providers;

public interface ISqliteConnectionManager : IDisposable, IAsyncDisposable
{
    SqliteConnectionLease Open();

    Task<SqliteConnectionLease> OpenAsync(CancellationToken cancellationToken);
}

public sealed class SqliteConnectionLease : IDisposable, IAsyncDisposable
{
    private readonly bool _closeOnDispose;
    public SqliteConnection Connection { get; }
    
    public SqliteConnectionLease(SqliteConnection connection, bool closeOnDispose)
    {
        _closeOnDispose = closeOnDispose;
        Connection = connection;
    }

    public void Dispose()
    {
        if (_closeOnDispose)
            Connection.Close();
    }

    public async ValueTask DisposeAsync()
    {
        if (_closeOnDispose)
            await Connection.CloseAsync();
    }
}