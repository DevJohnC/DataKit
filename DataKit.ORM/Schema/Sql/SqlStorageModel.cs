using System;
using System.Collections.Generic;
using DataKit.Modelling;
using DataKit.Modelling.TypeModels;

namespace DataKit.ORM.Schema.Sql
{
	public abstract class SqlStorageModel : IDataModel
	{
		protected SqlStorageModel(IReadOnlyList<IModelField> fields, string defaultTableName)
		{
			Fields = fields ?? throw new ArgumentNullException(nameof(fields));
			DefaultTableName = defaultTableName ?? throw new ArgumentNullException(nameof(defaultTableName));
		}

		public string DefaultTableName { get; }

		public IModelField this[string fieldName]
		{
			get
			{
				if (TryGetField(fieldName, out var field))
					return field;
				throw new KeyNotFoundException();
			}
		}

		public IReadOnlyList<IModelField> Fields { get; }

		public abstract IModelNode Accept(IModelVisitor visitor);
		public abstract bool TryGetField(string fieldName, out IModelField field);
	}

	public class SqlStorageModel<TEntity> : SqlStorageModel, IDataModel<SqlStorageModel<TEntity>, SqlStorageField<TEntity>>
		where TEntity : class
	{
		private readonly Dictionary<string, SqlStorageField<TEntity>> _fields =
			new Dictionary<string, SqlStorageField<TEntity>>();

		public SqlStorageModel(IReadOnlyList<SqlStorageField<TEntity>> fields, string defaultTableName) :
			base(fields, defaultTableName)
		{
			Fields = fields;
			foreach (var field in fields)
				_fields.Add(field.FieldName, field);
		}

		public new SqlStorageField<TEntity> this[string fieldName] => _fields[fieldName];

		SqlStorageField<TEntity> IDataModel<SqlStorageField<TEntity>>.this[string fieldName] => this[fieldName];

		public new IReadOnlyList<SqlStorageField<TEntity>> Fields { get; }

		IReadOnlyList<SqlStorageField<TEntity>> IDataModel<SqlStorageField<TEntity>>.Fields => Fields;

		public override IModelNode Accept(IModelVisitor visitor)
		{
			return visitor.VisitModel(this);
		}

		public bool TryGetField(string fieldName, out SqlStorageField<TEntity> field)
			=> _fields.TryGetValue(fieldName, out field);

		public override bool TryGetField(string fieldName, out IModelField field)
		{
			if (_fields.TryGetValue(fieldName, out var outField))
			{
				field = outField;
				return true;
			}

			field = default;
			return false;
		}
	}

	public abstract class SqlStorageField : IModelField
	{
		public DataType FieldType { get; }
		public string FieldName { get; }
		public bool CanWrite { get; }
		public bool CanRead { get; }
		public IDataModel FieldModel => null;
		public bool IsServerGenerated { get; }
		public string ColumnName { get; }
		public FieldGraphPath<PropertyField> TypeModelGraphPath { get; }
		public bool IsPrimaryKey { get; }
		public abstract bool HasDefaultValue { get; }

		public abstract bool RequiresJoin { get; }

		protected SqlStorageField(DataType fieldType, string fieldName, bool canWrite, bool canRead,
			bool isServerGenerated, string columnName, bool isPrimaryKey, FieldGraphPath<PropertyField> typeModelGraphPath)
		{
			FieldType = fieldType;
			FieldName = fieldName;
			CanWrite = canWrite;
			CanRead = canRead;
			IsServerGenerated = isServerGenerated;
			ColumnName = columnName;
			TypeModelGraphPath = typeModelGraphPath;
			IsPrimaryKey = isPrimaryKey;
		}

		public abstract IModelNode Accept(IModelVisitor visitor);
	}

	public abstract class SqlStorageField<TEntity> : SqlStorageField, IModelField<SqlStorageModel<TEntity>, SqlStorageField<TEntity>>
		where TEntity : class
	{
		protected SqlStorageField(DataType fieldType, string fieldName, bool canWrite, bool canRead,
			bool isServerGenerated, string columnName, bool isPrimaryKey, FieldGraphPath<PropertyField> typeModelGraphPath) :
			base(fieldType, fieldName, canWrite, canRead, isServerGenerated, columnName, isPrimaryKey, typeModelGraphPath)
		{
		}

		SqlStorageModel<TEntity> IModelField<SqlStorageModel<TEntity>, SqlStorageField<TEntity>>.FieldModel => null;

		public virtual JoinSpecification<TEntity> JoinSpecification => null;

		public override bool RequiresJoin => JoinSpecification != null;
	}

	public class SqlStorageField<TEntity, TValue> : SqlStorageField<TEntity>, IModelFieldOf<TValue>
		where TEntity : class
	{
		public SqlStorageField(string fieldName, bool canWrite, bool canRead,
			bool isServerGenerated, string columnName, Func<TValue> defaultValue,
			bool isPrimaryKey, FieldGraphPath<PropertyField> typeModelGraphPath) :
			base(DataType.GetTypeOf<TValue>(), fieldName, canWrite, canRead, isServerGenerated, columnName, isPrimaryKey, typeModelGraphPath)
		{
			DefaultValue = defaultValue;
		}

		public Func<TValue> DefaultValue { get; }

		public override bool HasDefaultValue => DefaultValue != null;

		public new DataType<TValue> FieldType { get; } = DataType.GetTypeOf<TValue>();

		public override IModelNode Accept(IModelVisitor visitor)
		{
			return visitor.VisitField(this);
		}
	}

	public class ForeignKeySqlStorageField<TLocalEntity, TForeignEntity, TValue> : SqlStorageField<TLocalEntity, TValue>
		where TLocalEntity : class
	{
		public ForeignKeySqlStorageField(string fieldName, bool canWrite, bool canRead, bool isServerGenerated, string columnName, Func<TValue> defaultValue, bool isPrimaryKey, FieldGraphPath<PropertyField> typeModelGraphPath) :
			base(fieldName, canWrite, canRead, isServerGenerated, columnName, defaultValue, isPrimaryKey, typeModelGraphPath)
		{
		}
	}

	public class ForeignSqlStorageField<TLocalEntity, TForeignEntity, TValue> : SqlStorageField<TLocalEntity, TValue>
		where TLocalEntity : class
	{
		public override JoinSpecification<TLocalEntity> JoinSpecification { get; }

		public ForeignSqlStorageField(string fieldName, bool canWrite, bool canRead, bool isServerGenerated,
			string columnName, Func<TValue> defaultValue, bool isPrimaryKey, FieldGraphPath<PropertyField> typeModelGraphPath,
			JoinSpecification<TLocalEntity> joinSpecification) :
			base(fieldName, canWrite, canRead, isServerGenerated, columnName, defaultValue, isPrimaryKey, typeModelGraphPath)
		{
			JoinSpecification = joinSpecification;
		}
	}
}
