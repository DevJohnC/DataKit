using System;

namespace DataKit.SQL.QueryExpressions
{
	public abstract class StatementQueryExpression : ExecutableQueryExpression
	{
		public override ExpressionType ExpressionType => ExpressionType.Statement;

		protected override QueryExpression Accept(QueryExpressionVisitor expressionVisitor)
			=> expressionVisitor.VisitStatement(this);
	}

	public class SelectStatementQueryExpression : StatementQueryExpression
	{
		public SelectStatementQueryExpression(
			ProjectionQueryExpression projection, FromQueryExpression from,
			JoinQueryExpression[] joins,
			WhereQueryExpression where, HavingQueryExpression having,
			OrderByQueryExpression[] orderBy, GroupByQueryExpression[] groupBy,
			LimitQueryExpression limit)
		{
			Projection = projection;
			From = from;
			Joins = joins;
			Where = where;
			Having = having;
			OrderBy = (orderBy == null || orderBy.Length < 1) ? null : new OrderByCollectionQueryExpression(orderBy);
			GroupBy = (groupBy == null || groupBy.Length < 1) ? null : new GroupByCollectionQueryExpression(groupBy);
			Limit = limit;
		}

		public ProjectionQueryExpression Projection { get; }

		public FromQueryExpression From { get; }
		public JoinQueryExpression[] Joins { get; }
		public WhereQueryExpression Where { get; }

		public HavingQueryExpression Having { get; }

		public new OrderByCollectionQueryExpression OrderBy { get; }

		public new GroupByCollectionQueryExpression GroupBy { get; }

		public new LimitQueryExpression Limit { get; }
	}

	public class InsertStatementQueryExpression : StatementQueryExpression
	{
		public InsertStatementQueryExpression(IntoQueryExpression into, ColumnIdentifierQueryExpression[] columns, QueryExpression[][] rowsExpressions)
		{
			Into = into ?? throw new ArgumentNullException(nameof(into));
			Columns = columns ?? throw new ArgumentNullException(nameof(columns));
			RowsExpressions = rowsExpressions ?? throw new ArgumentNullException(nameof(rowsExpressions));
		}

		public IntoQueryExpression Into { get; }
		public ColumnIdentifierQueryExpression[] Columns { get; }
		public QueryExpression[][] RowsExpressions { get; }
	}

	public class DeleteStatementQueryExpression : StatementQueryExpression
	{
		public DeleteStatementQueryExpression(FromQueryExpression from, WhereQueryExpression where)
		{
			From = from ?? throw new ArgumentNullException(nameof(from));
			Where = where;
		}

		public FromQueryExpression From { get; }
		public WhereQueryExpression Where { get; }
	}
}
