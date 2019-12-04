using System.Collections.Generic;

namespace DataKit.ORM.Schema.Sql.Policies
{
	public static class ModellingPolicyCollectionExtensions
	{
		public static void AddDefaults(this IList<ISqlModellingPolicy> policies)
		{
			policies.Add(StoreCompatibleSqlPrimitivesPolicy.Instance);
			policies.Add(IdIsPrimaryKeyPolicy.Instance);
			policies.Add(ReferenceSqlEntitiesForeignKeyPolicy.Instance);
		}
	}
}
