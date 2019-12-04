using DataKit.ORM.Schema;
using DataKit.ORM.Schema.Sql;
using System;
using System.Reflection;

namespace DataKit.ORM
{
	internal class DataContextSchemaFactory
	{
		private const string CONFIGURE_METHOD_NAME = "ConfigureSchema";

		public static DataSchema CreateSchemaFromDataContextType(Type dataContextType)
		{
			if (dataContextType.IsAbstract)
				throw new InvalidOperationException("Cannot create schema from abstract data context type.");

			var schemaBuilder = new DataSchemaBuilder();

			foreach (var property in dataContextType.GetDataSetProperties())
			{
				if (property.PropertyType.IsGenericType)
				{
					var genericArgs = property.PropertyType.GetGenericArguments();
					if (property.PropertyType.IsSqlDataSetWithEntityAndBusinessTypes())
					{
						var sqlEntityConfiguration = new SqlEntityConfiguration(
							genericArgs[0],
							genericArgs[1]
							);
						AutoConfigureEntityType(sqlEntityConfiguration);
						schemaBuilder.AddSqlEntity(sqlEntityConfiguration);
					}
					else if (property.PropertyType.IsSqlDataSetWithEntityType())
					{
						var sqlEntityConfiguration = new SqlEntityConfiguration(
							null,
							genericArgs[0]
							);
						AutoConfigureEntityType(sqlEntityConfiguration);
						schemaBuilder.AddSqlEntity(sqlEntityConfiguration);
					}
				}
			}

			AttemptCallConfigure(dataContextType, schemaBuilder);

			return schemaBuilder.Build();
		}

		private static void AttemptCallConfigure(Type dataContextType, DataSchemaBuilder dataSchemaBuilder)
		{
			var method = dataContextType.GetMethod(CONFIGURE_METHOD_NAME, BindingFlags.Public | BindingFlags.Static);
			if (method == null)
				return;

			var parameters = method.GetParameters();
			if (parameters.Length != 1 || parameters[0].ParameterType != typeof(DataSchemaBuilder))
				return;

			method.Invoke(null, new object[] { dataSchemaBuilder });
		}

		private static void AutoConfigureEntityType(SqlEntityConfiguration entityConfiguration)
		{
		}
	}
}
