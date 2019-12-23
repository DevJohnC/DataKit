using DataKit.ORM.Schema.Sql;
using DataKit.ORM.Sql.Expressions;
using DataKit.SQL.QueryExpressions;

namespace DataKit.ORM.Sql.QueryBuilding
{
	public class JoinBuilder<TEntity> : IAliasIdentifier
		where TEntity : class
	{
		public static readonly JoinBuilder<TEntity>[] Empty = new JoinBuilder<TEntity>[0];

		private string _rightAlias;

		public IAliasIdentifier Left { get; }

		public IAliasIdentifier Right { get; }

		public JoinDirection Direction { get; }
		public JoinColumnPair[] ColumnPairs { get; }

		public JoinSpecification<TEntity> Specification { get; }

		string IAliasIdentifier.AliasIdentifier => _rightAlias ?? Right.AliasIdentifier;

		public JoinBuilder(JoinSpecification<TEntity> specification, IAliasIdentifier left, IAliasIdentifier right, JoinDirection direction, params JoinColumnPair[] columnPairs)
		{
			Left = left;
			Right = right;
			Direction = direction;
			ColumnPairs = columnPairs;
			Specification = specification;
		}

		public JoinBuilder(JoinSpecification<TEntity> specification, IAliasIdentifier left, IAliasIdentifier right, string rightAlias, JoinDirection direction, params JoinColumnPair[] columnPairs) :
			this(specification, left, right, direction, columnPairs)
		{
			_rightAlias = rightAlias;
		}

		public JoinQueryExpression Build()
		{
			var rightIdentifier = QueryExpression.AliasReference(((IAliasIdentifier)this).AliasIdentifier);
			var leftIdentifier = QueryExpression.AliasReference(Left.AliasIdentifier);

			var onCondition = default(QueryExpression);
			foreach (var columnPair in ColumnPairs)
			{
				var newCondition = QueryExpression.AreEqual(
						QueryExpression.Column(columnPair.LeftColumnName, leftIdentifier),
						QueryExpression.Column(columnPair.RightColumnName, rightIdentifier)
						);
				onCondition = onCondition.AndAlso(newCondition);
			}

			return QueryExpression.Join(
				QueryExpression.As(
					QueryExpression.AliasReference(Right.AliasIdentifier), rightIdentifier.IdentifierName, out var _
					),
				onCondition,
				Direction
				);
		}
	}
}
