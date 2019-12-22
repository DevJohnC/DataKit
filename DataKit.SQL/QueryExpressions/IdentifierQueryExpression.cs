using System;

namespace DataKit.SQL.QueryExpressions
{
	public abstract class IdentifierQueryExpression : QueryExpression
	{
		protected IdentifierQueryExpression(string identifierName)
		{
			IdentifierName = identifierName ?? throw new ArgumentNullException(nameof(identifierName));
		}

		public override ExpressionType ExpressionType => ExpressionType.Identifier;

		public virtual string IdentifierName { get; }

		protected override QueryExpression Accept(QueryExpressionVisitor expressionVisitor)
			=> expressionVisitor.VisitIdentifier(this);
	}

	public abstract class NestedIdentifierQueryExpression : IdentifierQueryExpression
	{
		public IdentifierQueryExpression ParentIdentifier { get; }

		public NestedIdentifierQueryExpression(string identifierName, IdentifierQueryExpression parentIdentifier) : base(identifierName)
		{
			ParentIdentifier = parentIdentifier;
		}
	}

	public class TableIdentifierQueryExpression : IdentifierQueryExpression
	{
		public TableIdentifierQueryExpression(string tableName) : base(tableName)
		{
		}
	}

	public class AliasIdentifierQueryExpression : IdentifierQueryExpression
	{
		public AliasIdentifierQueryExpression(string identifierName) : base(identifierName)
		{
		}
	}

	public class ColumnIdentifierQueryExpression : NestedIdentifierQueryExpression
	{
		public ColumnIdentifierQueryExpression(string columnName, IdentifierQueryExpression parentIdentifier) : base(columnName, parentIdentifier)
		{
		}
	}
}
