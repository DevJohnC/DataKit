namespace DataKit.RelationalDatabases.QueryExpressions;

public sealed record SelectQueryExpression(
    ProjectionQueryExpression Projection,
    QueryExpression? From = null,
    QueryExpression? Where = null,
    LimitQueryExpression? Limit = null) : QueryExpression;