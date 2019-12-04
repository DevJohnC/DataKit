using DataKit.Modelling;
using DataKit.Modelling.TypeModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataKit.ORM.Schema.Sql.Policies
{
	public class ReferenceSqlEntitiesForeignKeyPolicy : ISqlModellingPolicy
	{
		public static ReferenceSqlEntitiesForeignKeyPolicy Instance { get; }
			= new ReferenceSqlEntitiesForeignKeyPolicy();

		public void Apply(BuildContext context, BuildStep buildStep, SqlEntityConfiguration sqlEntityConfiguration,
			SqlDataModelBuilder sqlDataModelBuilder)
		{
			if (buildStep != BuildStep.DefineRelationships)
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

					if (SqlTypeHelper.GetDataType(field.FieldType.Type) == null)
					{
						if (!IsRegisteredEntity(context, field.FieldType.Type))
						{
							ModelFields(field.FieldModel.Fields, fieldGraphPath.Path);
						}
						else
						{
							//  build a fk relationship
							var model = context.SqlBuilders.First(q => q.EntityType == field.FieldType.Type);
							var foreignKeyField = model.ModelledFields.FirstOrDefault(q => q.Options.IsPrimaryKey);
							if (foreignKeyField == null)
								throw new Exception("Referenced entity missing primary key field.");

							var fieldConfig = SqlFieldConfiguration.Create(field)
								.ColumnName($"{string.Join("_", fieldGraphPath.Path)}_{foreignKeyField.Options.ColumnName}")
								.DataType(foreignKeyField.Options.SqlDataType)
								.IsNullable(true)
								.ForeignKey(foreignKeyField.PropertyPath);
							fieldConfig.PropertyPath = fieldGraphPath;

							sqlDataModelBuilder.ModelledFields.Add(fieldConfig);
						}
					}
				}
			}
		}

		private static bool IsRegisteredEntity(BuildContext context, Type type)
			=> context.SqlBuilders.Any(q => q.EntityType == type);
	}
}
