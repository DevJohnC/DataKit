using System.Text;
using DataKit.RelationalDatabases.QueryExpressions;

namespace DataKit.RelationalDatabases.Providers;

public sealed class SqliteQueryWriter
{
    private readonly StringBuilder _queryText = new();
    private readonly List<SqlQueryParameter> _parameters = new();

    public SqlQuery Write(QueryExpression queryExpression)
    {
        switch (queryExpression)
        {
            case MultipleStatementsQueryExpression multipleStatementsQueryExpression:
            {
                WriteMultipleStatements(multipleStatementsQueryExpression);
                break;
            }
            case FieldNameQueryExpression fieldNameQueryExpression:
            {
                WriteFieldName(fieldNameQueryExpression);
                break;
            }
            case TableQueryExpression tableQueryExpression:
            {
                WriteTableName(tableQueryExpression);
                break;
            }
            case RawSqlQueryExpression rawSqlQueryExpression:
            {
                WriteRawSql(rawSqlQueryExpression);
                break;
            }
            case SelectQueryExpression selectQueryExpression:
            {
                WriteSelect(selectQueryExpression);
                break;
            }
            case ConstantQueryExpression constantQueryExpression:
            {
                WriteConstant(constantQueryExpression);
                break;
            }
        }

        return new SqlQuery(
            _queryText.ToString(),
            _parameters.Count == 0 ? 
                SqlQueryParametersCollection.Empty : 
                new SqlQueryParametersCollection(_parameters));
    }

    private void WriteMultipleStatements(MultipleStatementsQueryExpression queryExpression)
    {
        foreach (var statement in queryExpression.Statements)
        {
            Write(statement);
            _queryText.Append("; ");
        }
    }

    private void WriteFieldName(FieldNameQueryExpression queryExpression)
    {
        _queryText.Append($"[{queryExpression.FieldName}] ");
    }
    
    private void WriteTableName(TableQueryExpression queryExpression)
    {
        _queryText.Append($"[{queryExpression.TableName}] ");
    }

    private void WriteRawSql(RawSqlQueryExpression queryExpression)
    {
        _queryText.Append(queryExpression.RawSql);
    }

    private void WriteSelect(SelectQueryExpression queryExpression)
    {
        _queryText.Append("SELECT ");
        WriteProjection(queryExpression.Projection);
        WriteFrom(queryExpression.From);
        WriteLimit(queryExpression.Limit);
    }

    private void WriteProjection(ProjectionQueryExpression queryExpression)
    {
        if (queryExpression.Expressions.Count == 0)
        {
            _queryText.Append("* ");
            return;
        }
        
        foreach (var projectedField in queryExpression.Expressions)
        {
            Write(projectedField);
            _queryText.Append(", ");
        }
        
        RemoveHangingComma();
    }

    private void WriteConstant(ConstantQueryExpression queryExpression)
    {
        var parameterName = $"@param{_parameters.Count}";

        _queryText.Append(parameterName);
        _parameters.Add(new SqlQueryParameter(
            parameterName,
            queryExpression.Value));
    }

    private void WriteFrom(QueryExpression? queryExpression)
    {
        if (queryExpression == null)
            return;

        _queryText.Append("FROM ");

        switch (queryExpression)
        {
            case SelectQueryExpression selectQueryExpression:
            {
                _queryText.Append("(");
                WriteSelect(selectQueryExpression);
                _queryText.Append(") ");
                break;
            }
            case RawSqlQueryExpression rawSqlQueryExpression:
            {
                _queryText.Append("(");
                WriteRawSql(rawSqlQueryExpression);
                _queryText.Append(") ");
                break;
            }
            default:
                Write(queryExpression);
                break;
        }
    }

    private void WriteLimit(LimitQueryExpression? queryExpression)
    {
        if (queryExpression == null)
            return;

        _queryText.Append("LIMIT ");
        Write(queryExpression.Limit);

        if (queryExpression.HasOffset)
        {
            _queryText.Append(" OFFSET ");
            Write(queryExpression.Offset);
        }
    }

    private void RemoveHangingComma()
    {
        if (_queryText.ToString().EndsWith(", "))
            _queryText.Remove(_queryText.Length - 2, 2);
    }
}