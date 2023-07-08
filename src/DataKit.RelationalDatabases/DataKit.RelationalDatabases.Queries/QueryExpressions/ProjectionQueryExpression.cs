namespace DataKit.RelationalDatabases.QueryExpressions;

public sealed record ProjectionQueryExpression(IReadOnlyCollection<QueryExpression> Expressions) : QueryExpression
{
    public ProjectionQueryExpression(QueryExpression expression) :
        this(new[] { expression })
    {
    }
    
    public ProjectionQueryExpression(IEnumerable<QueryExpression> expression) :
        this(expression.ToList())
    {
    }
    
    public static readonly ProjectionQueryExpression All = new(Array.Empty<QueryExpression>());
}