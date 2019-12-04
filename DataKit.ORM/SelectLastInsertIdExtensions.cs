using DataKit.ORM.Sql;
using DataKit.ORM.Sql.Expressions;
using DataKit.ORM.Sql.QueryBuilding;
using Silk.Data.SQL.Expressions;
using System.Linq;

namespace DataKit.ORM
{
	public static class SelectLastInsertIdExtensions
	{
		public static SqlSelectOperation<TEntity, TView> LastInsertId<TEntity, TView>(this SqlSelectOperation<TEntity, TView> sql)
			where TEntity : class
			where TView : class
		{
			var whereBuilder = sql as IWhereQueryBuilder<TEntity>;
			var primaryKeyField = sql.DataSet.DataModel.StorageModel.Fields.FirstOrDefault(
				q => q.IsPrimaryKey && q.IsServerGenerated
				);
			if (primaryKeyField == null)
				throw new System.InvalidOperationException("To query last inserted ID a server generated primary key is required.");

			var expression = QueryExpression.Compare(
				QueryExpression.Column(primaryKeyField.ColumnName),
				ComparisonOperator.AreEqual,
				QueryExpression.LastInsertIdFunction()
				);
			whereBuilder.AndWhere(new SqlValueExpression<TEntity, bool>(expression, JoinBuilder<TEntity>.Empty));

			var entityBinding = sql.DataSet.Bindings.BindEntityToView<TView>();
			var storageBinding = sql.DataSet.DataModel.GetOutStorageBinding(entityBinding);

			var pkStorageBindingPair = storageBinding.FieldBindings.FirstOrDefault(q => ReferenceEquals(q.SourceField, primaryKeyField));
			if (pkStorageBindingPair == null)
				throw new System.InvalidOperationException("Primary key field isn't bound on the view type.");

			var pkEntityBindingPair = entityBinding.FieldBindings.FirstOrDefault(q => ReferenceEquals(q.TargetField, pkStorageBindingPair.TargetField));
			if (pkEntityBindingPair == null)
				throw new System.InvalidOperationException("Primary key field isn't bound on the view type.");

			entityBinding = new Mapping.Binding.DataModelBinding<Modelling.TypeModels.TypeModel<TEntity>, Modelling.TypeModels.PropertyField, Modelling.TypeModels.TypeModel<TView>, Modelling.TypeModels.PropertyField>(
				entityBinding.SourceModel,
				entityBinding.TargetModel,
				new[] { pkEntityBindingPair }
				);
			sql.Bind(entityBinding);

			return sql;
		}
	}
}
