namespace DataKit.RelationalDatabases.QueryExpressions;

public sealed record ProjectionQueryExpression(IReadOnlyList<QueryExpression> Expressions) : QueryExpression
{
    public static readonly ProjectionQueryExpression All = new(Array.Empty<QueryExpression>());
}