using System.Diagnostics.CodeAnalysis;

namespace DataKit.RelationalDatabases.QueryExpressions;

public record LimitQueryExpression(
    QueryExpression Limit,
    QueryExpression? Offset = null) : QueryExpression
{
    [MemberNotNullWhen(true, nameof(Offset))]
    public bool HasOffset => Offset != null;
}