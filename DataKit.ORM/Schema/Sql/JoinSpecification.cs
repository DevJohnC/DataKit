using DataKit.ORM.Sql.Expressions;
using DataKit.ORM.Sql.QueryBuilding;
using DataKit.SQL.QueryExpressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataKit.ORM.Schema.Sql
{
	public class JoinSpecification<TEntity>
		where TEntity : class
	{
		public JoinColumnPair[] JoinColumns { get; }

		public JoinSpecification<TEntity> RequiredJoin { get; }

		public Type EntityType { get; }

		public string JoinName { get; }

		public JoinSpecification(Type entityType, string joinName, IEnumerable<JoinColumnPair> joinColumns) :
			this(null, entityType, joinName, joinColumns)
		{
		}

		public JoinSpecification(JoinSpecification<TEntity> requiredJoin, Type entityType, string joinName,
			IEnumerable<JoinColumnPair> joinColumns) :
			this(requiredJoin, entityType, joinName, joinColumns.ToArray())
		{
		}

		public JoinSpecification(Type entityType, string joinName, params JoinColumnPair[] joinColumns) :
			this(null, entityType, joinName, joinColumns)
		{
		}

		public JoinSpecification(JoinSpecification<TEntity> requiredJoin, Type entityType, string joinName,
			params JoinColumnPair[] joinColumns)
		{
			RequiredJoin = requiredJoin;
			JoinColumns = joinColumns;
			EntityType = entityType;
			JoinName = joinName;
		}

		public JoinBuilder<TEntity> CreateJoin(IAliasIdentifier left, IAliasIdentifier right, string rightAlias)
		{
			return new JoinBuilder<TEntity>(this, left, right, rightAlias, JoinDirection.Left, JoinColumns);
		}

		internal IEnumerable<(JoinBuilder<TEntity> Builder, IAliasIdentifier Alias)> CreateJoin(DataSchema dataSchema, SqlStorageField<TEntity> storageField,
			IAliasIdentifier tableIdentifier)
		{
			if (RequiredJoin != null)
			{
				foreach (var parentJoin in RequiredJoin.CreateJoin(dataSchema, storageField, tableIdentifier))
					yield return parentJoin;
			}

			var joinedDataModel = dataSchema.Sql.SqlEntities.FirstOrDefault(q => q.EntityType == EntityType);
			var leftIdentifier = tableIdentifier;
			if (RequiredJoin != null)
			{
				leftIdentifier = new ImmutableIdentifier(RequiredJoin.JoinName);
			}
			var rightIdentifier = new ImmutableIdentifier(joinedDataModel.StorageModel.DefaultTableName);
			var aliasIdentifier = new ImmutableIdentifier(JoinName);

			var joinBuilder = CreateJoin(
				leftIdentifier,
				rightIdentifier,
				aliasIdentifier.AliasIdentifier
				);
			yield return (joinBuilder, aliasIdentifier);
		}

		private class ImmutableIdentifier : IAliasIdentifier
		{
			public string AliasIdentifier { get; }

			public ImmutableIdentifier(string aliasIdentifier)
			{
				AliasIdentifier = aliasIdentifier;
			}
		}
	}
}
