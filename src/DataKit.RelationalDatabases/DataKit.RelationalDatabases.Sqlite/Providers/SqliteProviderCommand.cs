using Microsoft.Data.Sqlite;

namespace DataKit.RelationalDatabases.Providers;

public sealed class SqliteProviderCommand : ProviderCommand
{
    private readonly SqliteConnectionLease _connectionLease;
    private readonly SqliteCommand _command;

    public SqliteProviderCommand(SqliteConnectionLease connectionLease, SqliteCommand command)
    {
        _connectionLease = connectionLease;
        _command = command;
    }
    
    public override RowReader ExecuteReader()
    {
        return new SqliteRowReader(_command.ExecuteReader());
    }

    public override async Task<RowReader> ExecuteReaderAsync(CancellationToken cancellationToken)
    {
        return new SqliteRowReader(await _command.ExecuteReaderAsync(cancellationToken));
    }

    public override void Dispose()
    {
        _command.Dispose();
        _connectionLease.Dispose();
    }

    public override async ValueTask DisposeAsync()
    {
        await _command.DisposeAsync();
        await _connectionLease.DisposeAsync();
    }
}