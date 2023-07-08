namespace DataKit.RelationalDatabases.QueryExpressions;

public abstract record QueryExpression
{
    public static SelectQueryExpression Select(
        QueryExpression projection,
        QueryExpression? from = null,
        QueryExpression? where = null,
        LimitQueryExpression? limit = null) => Select(
        new ProjectionQueryExpression(projection),
        from,
        where,
        limit);
    
    public static SelectQueryExpression Select(
        IEnumerable<QueryExpression> projection,
        QueryExpression? from = null,
        QueryExpression? where = null,
        LimitQueryExpression? limit = null) => Select(
        new ProjectionQueryExpression(projection),
        from,
        where,
        limit);
    
    public static SelectQueryExpression Select(
        IReadOnlyCollection<QueryExpression> projection,
        QueryExpression? from = null,
        QueryExpression? where = null,
        LimitQueryExpression? limit = null) => Select(
        new ProjectionQueryExpression(projection),
        from,
        where,
        limit);
    
    public static SelectQueryExpression Select(
        ProjectionQueryExpression projection,
        QueryExpression? from = null,
        QueryExpression? where = null,
        LimitQueryExpression? limit = null) => new SelectQueryExpression(projection, from, where, limit);

    public static ConstantQueryExpression Constant(object value) => new ConstantQueryExpression(value);
}