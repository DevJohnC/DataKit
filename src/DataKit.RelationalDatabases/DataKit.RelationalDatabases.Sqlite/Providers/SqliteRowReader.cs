using System.Data.Common;

namespace DataKit.RelationalDatabases.Providers;

public sealed class SqliteRowReader : RowReader
{
    private readonly DbDataReader _dbDataReader;

    public SqliteRowReader(DbDataReader dbDataReader)
    {
        _dbDataReader = dbDataReader;
    }

    public override bool Read()
    {
        return _dbDataReader.Read();
    }

    public override Task<bool> ReadAsync(CancellationToken cancellationToken)
    {
        return _dbDataReader.ReadAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _dbDataReader.Dispose();
    }

    public override ValueTask DisposeAsync()
    {
        return _dbDataReader.DisposeAsync();
    }
}