namespace DataKit.RelationalDatabases.QueryExpressions;

public sealed record FieldNameQueryExpression(string FieldName) : QueryExpression;