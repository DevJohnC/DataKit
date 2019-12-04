namespace DataKit.ORM.Schema.Sql.Policies
{
	public class ConfiguredFieldPolicy : ISqlModellingPolicy
	{
		public ConfiguredFieldPolicy(SqlFieldConfiguration fieldConfiguration)
		{
			FieldConfiguration = fieldConfiguration;
		}

		public SqlFieldConfiguration FieldConfiguration { get; }

		public void Apply(BuildContext context, BuildStep buildStep, SqlEntityConfiguration sqlEntityConfiguration,
			SqlDataModelBuilder sqlDataModelBuilder)
		{
			if (buildStep != BuildStep.DefineFields)
				return;

			sqlDataModelBuilder.ModelledFields.Add(FieldConfiguration);
		}
	}
}
