﻿using DataKit.SQL.Providers;
using DataKit.SQL.QueryExpressions;
using DataKit.SQL.Sqlite3.QueryExpressions;

namespace DataKit.SQL.Sqlite3
{
	public class Sqlite3QueryWriter : QueryWriter
	{
		public override void WriteExtension(QueryExpression queryExpression, QueryExpressionVisitor queryExpressionVisitor)
		{
			if (queryExpression is ISqlite3Extension sqlite3Extension)
				sqlite3Extension.Write(this, queryExpressionVisitor);
			base.WriteExtension(queryExpression, queryExpressionVisitor);
		}

		public override void WriteConcatFunction(ConcatFunctionCallQueryExpression concatFunctionCallQueryExpression, QueryExpressionVisitor queryExpressionVisitor)
		{
			WriteExpressionCollection(concatFunctionCallQueryExpression.Expressions, queryExpressionVisitor, seperator: " || ");
		}

		public override void WriteLastInsertedIdFunction(LastInsertedIdFunctionCallExpression lastInsertedIdFunctionCallExpression, QueryExpressionVisitor queryExpressionVisitor)
		{
			queryText.Append(" last_insert_rowid() ");
		}

		public void Append(string str)
			=> queryText.Append(str);
	}
}
