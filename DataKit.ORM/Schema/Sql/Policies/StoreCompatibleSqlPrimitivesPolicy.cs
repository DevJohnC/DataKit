using DataKit.Modelling;
using DataKit.Modelling.TypeModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataKit.ORM.Schema.Sql.Policies
{
	public class StoreCompatibleSqlPrimitivesPolicy : ISqlModellingPolicy
	{
		public static StoreCompatibleSqlPrimitivesPolicy Instance { get; }
			= new StoreCompatibleSqlPrimitivesPolicy();

		private static bool IsRegisteredEntity(BuildContext context, Type type)
			=> context.SqlBuilders.Any(q => q.EntityType == type);

		private static SqlFieldConfiguration CreateFieldConfiguration(PropertyField field, FieldGraphPath<PropertyField> graphPath)
		{
			var fieldConfig = SqlFieldConfiguration.Create(field)
				.ColumnName(string.Join("_", graphPath.Path))
				.DataType(SqlTypeHelper.GetDataType(field.FieldType.Type))
				.IsNullable(SqlTypeHelper.TypeIsNullable(field.FieldType.Type));
			fieldConfig.PropertyPath = graphPath;
			return fieldConfig;
		}

		private static bool FieldAlreadyModelled(IEnumerable<SqlFieldConfiguration> modelledFields, FieldGraphPath<PropertyField> graphPath)
			=> modelledFields.Any(q => q.PropertyPath.Equals(graphPath));

		public void Apply(BuildContext context, BuildStep buildStep, SqlEntityConfiguration sqlEntityConfiguration,
			SqlDataModelBuilder sqlDataModelBuilder)
		{
			if (buildStep != BuildStep.DefineFields)
				return;

			var typeModel = TypeModel.GetModelOf(sqlEntityConfiguration.EntityType);
			ModelFields(typeModel.Fields, new string[0]);

			void ModelFields(IReadOnlyList<PropertyField> fields, string[] parentPath)
			{
				foreach (var field in fields)
				{
					var fieldGraphPath = new FieldGraphPath<PropertyField>(
						parentPath.Concat(new[] { field.FieldName }).ToArray(),
						field
						);

					if (FieldAlreadyModelled(sqlDataModelBuilder.ModelledFields, fieldGraphPath))
						continue;

					if (SqlTypeHelper.GetDataType(field.FieldType.Type) == null)
					{
						//  not an SQL primitive type, if it's not defined in builder auto-config sub properties
						if (IsRegisteredEntity(context, field.FieldType.Type))
							continue;

						ModelFields(field.FieldModel.Fields, fieldGraphPath.Path);
					}
					else
					{
						sqlDataModelBuilder.ModelledFields.Add(CreateFieldConfiguration(field, fieldGraphPath));
					}
				}
			}
		}
	}
}
