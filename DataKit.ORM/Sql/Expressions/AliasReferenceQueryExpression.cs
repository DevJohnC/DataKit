using DataKit.SQL.QueryExpressions;

namespace DataKit.ORM.Sql.Expressions
{
	public class AliasReferenceQueryExpression : IdentifierQueryExpression
	{
		public IAliasIdentifier AliasIdentifier { get; }

		public override string IdentifierName => AliasIdentifier.AliasIdentifier;

		public AliasReferenceQueryExpression(IAliasIdentifier aliasIdentifier) : base(null)
		{
			AliasIdentifier = aliasIdentifier;
		}

		protected override QueryExpression Accept(QueryExpressionVisitor expressionVisitor)
		{
			return expressionVisitor.Visit(
				QueryExpression.AliasReference(AliasIdentifier.AliasIdentifier)
				);
		}
	}
}
