using DataKit.ORM.Schema.Sql;
using DataKit.ORM.Sql.Expressions;
using Silk.Data.SQL.Expressions;
using System;

namespace DataKit.ORM.Sql.QueryBuilding
{
	public abstract class SqlQueryBuilder
	{
		public SqlServerFunctions ServerFunctions { get; }

		public abstract QueryExpression BuildQuery();
	}

	public abstract class SqlQueryBuilder<TEntity> : SqlQueryBuilder
		where TEntity : class
	{
		protected readonly SqlDataModel<TEntity> DataModel;

		private ValueConverter<TEntity> _valueConverter;
		protected ValueConverter<TEntity> ValueConverter
		{
			get
			{
				if (_valueConverter == null)
					_valueConverter = new ValueConverter<TEntity>(DataModel, this as IAliasIdentifier);
				return _valueConverter;
			}
		}

		private ConditionConverter<TEntity> _conditionConverter;
		protected ConditionConverter<TEntity> ConditionConverter
		{
			get
			{
				if (_conditionConverter == null)
					_conditionConverter = new ConditionConverter<TEntity>(DataModel, this as IAliasIdentifier);
				return _conditionConverter;
			}
		}

		protected SqlQueryBuilder(SqlDataModel<TEntity> dataModel)
		{
			DataModel = dataModel ?? throw new ArgumentNullException(nameof(dataModel));
		}
	}
}
