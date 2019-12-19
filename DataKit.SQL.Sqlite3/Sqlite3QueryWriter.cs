using DataKit.SQL.Providers;
using DataKit.SQL.QueryExpressions;
using DataKit.SQL.Sqlite3.QueryExpressions;

namespace DataKit.SQL.Sqlite3
{
	public class Sqlite3QueryWriter : QueryWriter
	{
		private readonly bool _convertGuidsToText;

		public Sqlite3QueryWriter(bool convertGuidsToText)
		{
			_convertGuidsToText = convertGuidsToText;
		}

		public override void WriteExtension(QueryExpression queryExpression, QueryExpressionVisitor queryExpressionVisitor)
		{
			if (queryExpression is ISqlite3Extension sqlite3Extension)
				sqlite3Extension.Write(this, queryExpressionVisitor);
			base.WriteExtension(queryExpression, queryExpressionVisitor);
		}

		public void Append(string str)
			=> queryText.Append(str);
	}
}
