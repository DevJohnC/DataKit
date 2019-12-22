using DataKit.Modelling;
using DataKit.Modelling.TypeModels;
using DataKit.ORM.Sql;
using System;

namespace DataKit.ORM.Schema.Sql
{
	public abstract class SqlFieldConfiguration
	{
		internal FieldGraphPath<PropertyField> PropertyPath { get; set; }

		public SqlFieldOptions Options { get; }

		public SqlFieldConfiguration(SqlFieldOptions options)
		{
			Options = options;
		}

		public SqlFieldConfiguration ColumnName(string columnName)
		{
			Options.ColumnName = columnName;
			return this;
		}

		public SqlFieldConfiguration IsNullable(bool isNullable)
		{
			Options.IsNullable = isNullable;
			return this;
		}

		public SqlFieldConfiguration DataType(SqlDataType dataType)
		{
			Options.SqlDataType = dataType;
			return this;
		}

		public SqlFieldConfiguration PrimaryKey()
		{
			PrimaryKeyImpl();
			return this;
		}

		public SqlFieldConfiguration ForeignKey(FieldGraphPath<PropertyField> foreignKeyFieldPath)
		{
			Options.IsForeignKey = true;
			Options.ForeignKeyFieldPath = foreignKeyFieldPath;
			return this;
		}

		protected abstract void PrimaryKeyImpl();

		public static SqlFieldConfiguration Create(IModelField modelField)
		{
			return ((SqlFieldConfigurationWrapper)new FieldConverter().Visit(modelField))
				.SqlFieldConfiguration;
		}

		private struct FieldConverter : IModelVisitor
		{
			public IModelNode Visit(IModelNode modelNode)
			{
				return modelNode?.Accept(this);
			}

			public IModelNode VisitExtension(IModelNode node)
			{
				throw new NotImplementedException();
			}

			public IModelNode VisitField<T>(IModelFieldOf<T> field)
			{
				return new SqlFieldConfigurationWrapper(
					new SqlFieldConfiguration<T>()
					);
			}

			public IModelNode VisitModel(IDataModel model)
			{
				throw new NotImplementedException();
			}
		}

		private struct SqlFieldConfigurationWrapper : IModelNode
		{
			public SqlFieldConfigurationWrapper(SqlFieldConfiguration sqlFieldConfiguration)
			{
				SqlFieldConfiguration = sqlFieldConfiguration;
			}

			public SqlFieldConfiguration SqlFieldConfiguration { get; }

			public IModelNode Accept(IModelVisitor visitor)
			{
				return visitor.VisitExtension(this);
			}
		}
	}

	public class SqlFieldConfiguration<T> : SqlFieldConfiguration
	{
		public new SqlFieldOptions<T> Options { get; }

		public SqlFieldConfiguration() :
			this(new SqlFieldOptions<T>())
		{
		}

		private SqlFieldConfiguration(SqlFieldOptions<T> options) :
			base(options)
		{
			Options = options;
		}

		public new SqlFieldConfiguration<T> ColumnName(string columnName)
		{
			base.ColumnName(columnName);
			return this;
		}

		public new SqlFieldConfiguration<T> IsNullable(bool isNullable)
		{
			base.IsNullable(isNullable);
			return this;
		}

		public new SqlFieldConfiguration<T> DataType(SqlDataType dataType)
		{
			base.DataType(dataType);
			return this;
		}

		public SqlFieldConfiguration<T> DefaultValue(Func<T> defaultValue)
		{
			Options.DefaultValue = defaultValue;
			return this;
		}

		public SqlFieldConfiguration<T> NoDefaultValue()
		{
			Options.DefaultValue = null;
			return this;
		}

		public new SqlFieldConfiguration<T> PrimaryKey()
		{
			base.PrimaryKey();
			return this;
		}

		public new SqlFieldConfiguration<T> ForeignKey(FieldGraphPath<PropertyField> foreignKeyFieldPath)
		{
			base.ForeignKey(foreignKeyFieldPath);
			return this;
		}

		protected override void PrimaryKeyImpl()
		{
			Options.IsPrimaryKey = true;
			if (typeof(int) == typeof(T) || typeof(long) == typeof(T) || typeof(short) == typeof(T))
			{
				Options.IsServerGenerated = true;
			}
			else if (typeof(Guid) == typeof(T))
			{
				//  todo: remove the need for the boxing as object here
				Options.DefaultValue = () => (T)(object)Guid.NewGuid();
			}
		}
	}

	public abstract class SqlFieldOptions
	{
		public SqlDataType SqlDataType { get; set; }

		public bool IsNullable { get; set; }

		public bool IsServerGenerated { get; set; }

		public string ColumnName { get; set; }

		public bool IsPrimaryKey { get; set; }
		public bool IsForeignKey { get; set; }
		public FieldGraphPath<PropertyField> ForeignKeyFieldPath { get; set; }
	}

	public class SqlFieldOptions<T> : SqlFieldOptions
	{
		public Func<T> DefaultValue { get; set; } = () => default;
	}
}
