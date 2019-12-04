namespace DataKit.ORM.Sql.QueryBuilding
{
	public interface IQueryBuilder<TEntity>
		where TEntity : class
	{
		IQueryBuilder<TEntity> Table(string tableName);
	}
}
