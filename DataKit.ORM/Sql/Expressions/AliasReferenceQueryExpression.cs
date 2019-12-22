using DataKit.SQL.QueryExpressions;

namespace DataKit.ORM.Sql.Expressions
{
	public class AliasReferenceQueryExpression : QueryExpression, IExtensionExpression
	{
		public IAliasIdentifier AliasIdentifier { get; }

		public override ExpressionNodeType NodeType => ExpressionNodeType.Extension;

		public AliasReferenceQueryExpression(IAliasIdentifier aliasIdentifier)
		{
			AliasIdentifier = aliasIdentifier;
		}

		public void Visit(QueryExpressionVisitor visitor)
		{
			visitor.Visit(new AliasIdentifierExpression(AliasIdentifier.AliasIdentifier));
		}
	}
}
