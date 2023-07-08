namespace DataKit.RelationalDatabases.QueryExpressions;

public sealed record ConstantQueryExpression(object Value) : QueryExpression;