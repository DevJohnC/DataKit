using DataKit.Mapping.Binding;
using DataKit.Modelling.TypeModels;
using System;
using System.Linq.Expressions;

namespace DataKit.ORM.Sql.QueryBuilding
{
	public interface IAssignmentQueryBuilder<TEntity> : IQueryBuilder<TEntity>
		where TEntity : class
	{
		FieldAssignmentBuilder<TEntity> Assignments { get; }

		IAssignmentQueryBuilder<TEntity> Set<TProperty>(Expression<Func<TEntity, TProperty>> fieldSelector, TProperty value);

		IAssignmentQueryBuilder<TEntity> Set<TProperty>(
			Expression<Func<TEntity, TProperty>> fieldSelector, Expression<Func<TEntity, TProperty>> valueExpression
			);

		IAssignmentQueryBuilder<TEntity> Set(TEntity entity);

		IAssignmentQueryBuilder<TEntity> Set<TView>(
			TView entityView,
			DataModelBinding<TypeModel<TView>, PropertyField, TypeModel<TEntity>, PropertyField> entityBinding
			)
			where TView : class;
	}
}
