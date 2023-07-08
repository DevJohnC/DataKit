using DataKit.RelationalDatabases.QueryExpressions;
using Microsoft.Data.Sqlite;

namespace DataKit.RelationalDatabases.Providers;

public sealed class SqliteRelationalDatabaseProvider : IRelationalDatabaseProvider
{
    private readonly ISqliteConnectionManager _connectionManager;
    
    private SqliteRelationalDatabaseProvider(ISqliteConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }
    
    public IEnumerable<RowReader> Execute(SqlQuery sqlQuery)
    {
        return new SyncQuery(this, sqlQuery);
    }

    public IAsyncEnumerable<RowReader> ExecuteAsync(SqlQuery sqlQuery)
    {
        return new AsyncQuery(this, sqlQuery);
    }

    public ProviderCommand CreateCommand(SqlQuery sqlQuery)
    {
        SqliteConnectionLease? connectionLease = null;
        SqliteCommand? command = null;
        try
        {
            connectionLease = _connectionManager.Open();
            command = CreateCommand(connectionLease, sqlQuery);
            return new SqliteProviderCommand(connectionLease, command);
        }
        catch
        {
            command?.Dispose();
            connectionLease?.Dispose();
            throw;
        }
    }

    public async Task<ProviderCommand> CreateCommandAsync(SqlQuery sqlQuery, CancellationToken cancellationToken)
    {
        SqliteConnectionLease? connectionLease = null;
        SqliteCommand? command = null;
        try
        {
            connectionLease = await _connectionManager.OpenAsync(cancellationToken);
            command = CreateCommand(connectionLease, sqlQuery);
            return new SqliteProviderCommand(connectionLease, command);
        }
        catch
        {
            if (command != null)
                await command.DisposeAsync();
            if (connectionLease != null)
                await connectionLease.DisposeAsync();
            throw;
        }
    }

    public SqlQuery CreateSqlQuery(QueryExpression queryExpression)
    {
        return new SqliteQueryWriter().Write(queryExpression);
    }

    private SqliteCommand CreateCommand(SqliteConnectionLease connectionLease, SqlQuery sqlQuery)
    {
        var command = connectionLease.Connection.CreateCommand();
        command.CommandText = sqlQuery.QueryText;
        foreach (var parameter in sqlQuery.Parameters)
        {
            var cmdParameter = command.CreateParameter();
            cmdParameter.ParameterName = parameter.Name;
            cmdParameter.Value = parameter.Value;
            if (parameter.DbType is not null)
                cmdParameter.DbType = parameter.DbType.Value;
            command.Parameters.Add(cmdParameter);
        }

        return command;
    }

    public void Dispose()
    {
        _connectionManager.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return _connectionManager.DisposeAsync();
    }

    public static SqliteRelationalDatabaseProvider CreateInMemory()
    {
        return new SqliteRelationalDatabaseProvider(new InMemorySqliteConnectionManager());
    }
}