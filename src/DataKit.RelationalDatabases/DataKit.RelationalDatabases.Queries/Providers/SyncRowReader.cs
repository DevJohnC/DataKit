using System.Collections;

namespace DataKit.RelationalDatabases.Providers;

public sealed class SyncRowReader : IEnumerator<RowReader>
{
    private readonly ProviderCommand _command;
    private readonly RowReader _reader;
    
    public RowReader Current => _reader;

    object IEnumerator.Current => Current;

    public SyncRowReader(ProviderCommand command, RowReader rowReader)
    {
        _command = command;
        _reader = rowReader;
    }
    
    public bool MoveNext()
    {
        return _reader.Read();
    }

    public void Reset()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        _reader.Dispose();
        _command.Dispose();
    }
}