using DataKit.SQL.QueryExpressions;
using System.Data.Common;

namespace DataKit.SQL.Providers
{
	public interface IQueryProviderCommon
	{
		(string Sql, ParameterBag parameters) ConvertQuery(QueryExpression queryExpression);

		DbCommand CreateCommand(DbConnection dbConnection, string sql, ParameterBag parameters);
	}
}
