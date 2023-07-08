using System.Collections;
using System.Data;

namespace DataKit.RelationalDatabases;

public sealed class SqlQuery
{
    public SqlQueryParametersCollection Parameters { get; }
    
    public string QueryText { get; }
    
    public SqlQuery(string queryText)
    {
        QueryText = queryText;
        Parameters = SqlQueryParametersCollection.Empty;
    }
    
    public SqlQuery(string queryText, SqlQueryParametersCollection parameters)
    {
        QueryText = queryText;
        Parameters = parameters;
    }
}

public sealed record SqlQueryParameter(string Name, object Value, DbType? DbType = null);

public sealed class SqlQueryParametersCollection : IReadOnlyList<SqlQueryParameter>
{
    public static readonly SqlQueryParametersCollection Empty = new();
    
    private readonly List<SqlQueryParameter> _parameters;
    
    public int Count => _parameters.Count;

    public SqlQueryParameter this[int index] => _parameters[index];

    private SqlQueryParametersCollection()
    {
        _parameters = new List<SqlQueryParameter>(0);
    }
    
    public SqlQueryParametersCollection(IEnumerable<SqlQueryParameter> parameters)
    {
        if (parameters is IReadOnlyCollection<SqlQueryParameter> collection)
        {
            _parameters = new List<SqlQueryParameter>(collection.Count);
            _parameters.AddRange(parameters);
        }
        else
        {
            _parameters = new List<SqlQueryParameter>(parameters);
        }
    }
    
    public IEnumerator<SqlQueryParameter> GetEnumerator()
    {
        return _parameters.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}