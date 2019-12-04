using DataKit.ORM.Schema.Sql;
using System.Collections.Generic;

namespace DataKit.ORM.Schema
{
	public class BuildContext
	{
		public List<SqlDataModelBuilder> SqlBuilders { get; }
			= new List<SqlDataModelBuilder>();
	}

	public enum BuildStep
	{
		DefineFields,
		DefineRelationships
	}
}
