namespace DataKit.RelationalDatabases.QueryExpressions;

public sealed record RawSqlQueryExpression(string RawSql) : QueryExpression;