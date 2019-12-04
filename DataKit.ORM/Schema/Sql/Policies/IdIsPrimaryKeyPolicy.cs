using System;
using System.Linq;

namespace DataKit.ORM.Schema.Sql.Policies
{
	public class IdIsPrimaryKeyPolicy : ISqlModellingPolicy
	{
		public static IdIsPrimaryKeyPolicy Instance { get; }
			= new IdIsPrimaryKeyPolicy();

		public void Apply(BuildContext context, BuildStep buildStep, SqlEntityConfiguration sqlEntityConfiguration,
			SqlDataModelBuilder sqlDataModelBuilder)
		{
			if (buildStep != BuildStep.DefineFields)
				return;

			if (sqlDataModelBuilder.ModelledFields.Any(q => q.Options.IsPrimaryKey))
				return;

			var idField = sqlDataModelBuilder.ModelledFields.FirstOrDefault(
				q => q.PropertyPath.Path.SequenceEqual(new[] { "Id" })
				);
			if (idField == null)
				return;

			idField.PrimaryKey();
		}
	}
}
