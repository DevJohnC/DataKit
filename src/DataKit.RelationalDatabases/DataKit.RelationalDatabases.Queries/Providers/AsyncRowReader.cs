namespace DataKit.RelationalDatabases.Providers;

public sealed class AsyncRowReader : IAsyncEnumerator<RowReader>
{
    private readonly ProviderCommand _command;
    private readonly RowReader _reader;
    private readonly CancellationToken _cancellationToken;

    public RowReader Current => _reader;

    public AsyncRowReader(ProviderCommand command, RowReader rowReader, CancellationToken cancellationToken)
    {
        _command = command;
        _reader = rowReader;
        _cancellationToken = cancellationToken;
    }

    public async ValueTask<bool> MoveNextAsync()
    {
        return await _reader.ReadAsync(_cancellationToken);
    }
    
    public async ValueTask DisposeAsync()
    {
        await _command.DisposeAsync();
        await _reader.DisposeAsync();
    }
}