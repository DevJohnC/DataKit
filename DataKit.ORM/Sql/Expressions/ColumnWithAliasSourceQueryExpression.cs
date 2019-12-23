using DataKit.ORM.Schema.Sql;
using DataKit.SQL.QueryExpressions;

namespace DataKit.ORM.Sql.Expressions
{
	public class ColumnWithAliasSourceQueryExpression : NestedIdentifierQueryExpression
	{
		public string ColumnName { get; }

		public IAliasIdentifier SourceIdentifier { get; }

		public SqlStorageField SqlStorageField { get; }

		public ColumnWithAliasSourceQueryExpression(string columnName, IAliasIdentifier sourceIdentifier,
			SqlStorageField sqlStorageField) : base(columnName, sourceIdentifier == null ? null : new AliasReferenceQueryExpression(sourceIdentifier))
		{
			ColumnName = columnName;
			SourceIdentifier = sourceIdentifier;
			SqlStorageField = sqlStorageField;
		}

		protected override QueryExpression Accept(QueryExpressionVisitor expressionVisitor)
		{
			return expressionVisitor.Visit(
				QueryExpression.Column(ColumnName, ParentIdentifier)
				);
		}
	}
}
