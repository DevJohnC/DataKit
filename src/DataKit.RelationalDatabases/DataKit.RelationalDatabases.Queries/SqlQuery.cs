using System.Collections;
using System.Data;

namespace DataKit.RelationalDatabases;

public sealed class SqlQuery
{
    public SqlQueryParametersCollection Parameters { get; }
    
    public string QueryText { get; }
}

public sealed record SqlQueryParameter(string Name, object Value, DbType? DbType = null);

public sealed class SqlQueryParametersCollection : IReadOnlyList<SqlQueryParameter>
{
    private readonly List<SqlQueryParameter> _parameters = new();
    
    public int Count => _parameters.Count;

    public SqlQueryParameter this[int index] => _parameters[index];
    
    public IEnumerator<SqlQueryParameter> GetEnumerator()
    {
        return _parameters.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}