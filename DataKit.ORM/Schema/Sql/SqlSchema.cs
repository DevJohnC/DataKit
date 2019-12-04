using DataKit.ORM.Sql.Expressions.MethodConversion;
using System.Collections.Generic;
using System.Linq;

namespace DataKit.ORM.Schema.Sql
{
	public class SqlSchema : IDataSchema, IDataSchemaAssignment
	{
		public DataSchema DataSchema { get; private set; }

		public IReadOnlyList<SqlDataModel> SqlEntities { get; }

		public IReadOnlyList<ISqlMethodCallConverter> SqlMethodConverters { get; }

		public SqlSchema(IReadOnlyList<SqlDataModel> sqlEntities, IReadOnlyList<ISqlMethodCallConverter> sqlMethodCallConverters)
		{
			SqlMethodConverters = new List<ISqlMethodCallConverter>(
				new[] { new BuiltInMethodCallConverter() }.Concat(sqlMethodCallConverters)
				);
			SqlEntities = sqlEntities;
		}

		void IDataSchemaAssignment.SetDataSchemaAndSeal(DataSchema dataSchema)
		{
			if (DataSchema != null)
				throw new System.InvalidOperationException("DataSchema is sealed.");
			DataSchema = dataSchema;

			foreach (var dataSchemaAssignment in SqlEntities.OfType<IDataSchemaAssignment>())
				dataSchemaAssignment.SetDataSchemaAndSeal(dataSchema);
		}
	}
}
