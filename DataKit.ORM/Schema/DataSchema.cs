using DataKit.ORM.Schema.Sql;
using System.Collections.Generic;
using System.Linq;

namespace DataKit.ORM.Schema
{
	public class DataSchema
	{
		public IReadOnlyList<IDataSchema> DataSchemas { get; }

		public SqlSchema Sql { get; }

		public DataSchema(params IDataSchema[] dataSchemas)
		{
			DataSchemas = dataSchemas;
			foreach (var dataSchemaAssignment in DataSchemas.OfType<IDataSchemaAssignment>())
				dataSchemaAssignment.SetDataSchemaAndSeal(this);
			Sql = DataSchemas.OfType<SqlSchema>().FirstOrDefault();
		}
	}
}
