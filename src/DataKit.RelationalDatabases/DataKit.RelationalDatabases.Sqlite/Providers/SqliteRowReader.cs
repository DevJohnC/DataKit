using Microsoft.Data.Sqlite;

namespace DataKit.RelationalDatabases.Providers;

public sealed class SqliteRowReader : RowReader
{
    public SqliteRowReader(SqliteDataReader dbDataReader) :
        base(dbDataReader)
    {
    }
}