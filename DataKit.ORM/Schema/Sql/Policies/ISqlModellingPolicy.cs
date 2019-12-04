using System.Collections.Generic;

namespace DataKit.ORM.Schema.Sql.Policies
{
	public interface ISqlModellingPolicy
	{
		void Apply(
			BuildContext context,
			BuildStep buildStep,
			SqlEntityConfiguration sqlEntityConfiguration,
			SqlDataModelBuilder sqlDataModelBuilder);
	}
}
