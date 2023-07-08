using Microsoft.Data.Sqlite;

namespace DataKit.RelationalDatabases.Providers;

public sealed class InMemorySqliteConnectionManager : ISqliteConnectionManager
{
    private readonly SqliteConnection _connection;

    public InMemorySqliteConnectionManager()
    {
        var connStr = new SqliteConnectionStringBuilder();
        connStr.DataSource = ":memory:";

        _connection = new SqliteConnection(connStr.ConnectionString);
        _connection.Open();
    }
    
    public void Dispose()
    {
        _connection.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return _connection.DisposeAsync();
    }

    public SqliteConnectionLease Open()
    {
        return new SqliteConnectionLease(_connection, false);
    }

    public Task<SqliteConnectionLease> OpenAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(new SqliteConnectionLease(_connection, false));
    }
}