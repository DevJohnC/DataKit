using DataKit.ORM.Schema.Sql;
using Silk.Data.SQL.Expressions;

namespace DataKit.ORM.Sql.Expressions
{
	public class ColumnWithAliasSourceQueryExpression : QueryExpression, IExtensionExpression
	{
		public string ColumnName { get; }

		public IAliasIdentifier SourceIdentifier { get; }

		public SqlStorageField SqlStorageField { get; }

		public override ExpressionNodeType NodeType => ExpressionNodeType.Extension;

		public ColumnWithAliasSourceQueryExpression(string columnName, IAliasIdentifier sourceIdentifier,
			SqlStorageField sqlStorageField)
		{
			ColumnName = columnName;
			SourceIdentifier = sourceIdentifier;
			SqlStorageField = sqlStorageField;
		}

		public void Visit(QueryExpressionVisitor visitor)
		{
			var source = default(AliasIdentifierExpression);
			if (SourceIdentifier != null)
				source = new AliasIdentifierExpression(SourceIdentifier.AliasIdentifier);

			visitor.Visit(new ColumnExpression(
				source,
				ColumnName
				));
		}
	}
}
