using System;
using System.Collections.Generic;
using System.Reflection;
using DataKit.Mapping;
using DataKit.Modelling;
using DataKit.Modelling.TypeModels;

namespace DataKit.ORM.Schema.Sql
{
	public class SqlEntityModel<TEntity> : TypeModel<TEntity>
		where TEntity : class
	{
		public new IReadOnlyList<SqlEntityField<TEntity>> Fields { get; }

		public SqlEntityModel(IReadOnlyList<SqlEntityField<TEntity>> fields) : base(fields)
		{
			Fields = fields;
		}

		public bool TryGetField(string fieldName, out SqlEntityField<TEntity> field)
		{
			if (!base.TryGetField(fieldName, out PropertyField propertyField))
			{
				field = default;
				return false;
			}

			field = propertyField as SqlEntityField<TEntity>;
			return true;
		}
	}

	public abstract class SqlEntityField : PropertyField
	{
		public override TypeModel FieldModel => GetFieldModel();

		protected abstract TypeModel GetFieldModel();
	}

	public abstract class SqlEntityField<TEntity> : SqlEntityField
		where TEntity : class
	{
		public new SqlEntityModel<TEntity> FieldModel { get; internal set; }

		protected override TypeModel GetFieldModel()
			=> FieldModel;

		public abstract FieldGraphPath<PropertyField> TypeModelGraphPath { get; }

		public abstract object ReadValue(IDataModelReader<TypeModel<TEntity>, PropertyField> reader);
	}

	public class SqlEntityField<TEntity, TValue> : SqlEntityField<TEntity>, IModelFieldOf<TValue>
		where TEntity : class
	{
		public override string FieldName { get; }

		public override bool CanWrite { get; }

		public override bool CanRead { get; }

		DataType<TValue> IModelFieldOf<TValue>.FieldType { get; } = DataType.GetTypeOf<TValue>();

		public override FieldGraphPath<PropertyField> TypeModelGraphPath { get; }

		public SqlEntityField(string fieldName, bool canWrite, bool canRead, PropertyInfo propertyInfo,
			FieldGraphPath<PropertyField> typeModelGraphPath)
		{
			FieldName = fieldName;
			CanWrite = canWrite;
			CanRead = canRead;
			Property = propertyInfo;
			TypeModelGraphPath = typeModelGraphPath;
		}

		public override IModelNode Accept(IModelVisitor visitor)
		{
			return visitor.VisitField(this);
		}

		protected override DataType GetDataType()
		{
			return DataType.GetTypeOf<TValue>();
		}

		protected override void InitalizeFromPropertyInfoOverride(PropertyInfo propertyInfo)
		{
		}

		public override object ReadValue(IDataModelReader<TypeModel<TEntity>, PropertyField> reader)
		{
			return reader.ReadField<TValue>(this);
		}
	}
}
