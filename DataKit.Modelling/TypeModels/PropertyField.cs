using System;
using System.Reflection;

namespace DataKit.Modelling.TypeModels
{
	public abstract class PropertyField : IModelField<TypeModel, PropertyField>
	{
		public DataType FieldType => GetDataType();
		public abstract string FieldName { get; }
		public abstract bool CanWrite { get; }
		public abstract bool CanRead { get; }

		public PropertyInfo Property { get; protected set; }

		public virtual TypeModel FieldModel => TypeModel.GetModelOf(FieldType.Type);

		IDataModel IModelField.FieldModel => FieldModel;

		private void InitalizeFromPropertyInfo(PropertyInfo propertyInfo)
		{
			Property = propertyInfo;
			InitalizeFromPropertyInfoOverride(propertyInfo);
		}

		protected abstract DataType GetDataType();

		protected abstract void InitalizeFromPropertyInfoOverride(PropertyInfo propertyInfo);

		/// <summary>
		/// Create a PropertyInfoField from a PropertyInfo instance.
		/// </summary>
		/// <param name="propertyInfo"></param>
		/// <returns></returns>
		public static PropertyField CreateFromPropertyInfo(PropertyInfo propertyInfo)
		{
			var ret = Activator.CreateInstance(typeof(PropertyField<>).MakeGenericType(
				propertyInfo.PropertyType
				)) as PropertyField;
			ret.InitalizeFromPropertyInfo(propertyInfo);
			return ret;
		}

		public abstract IModelNode Accept(IModelVisitor visitor);
	}

	public class PropertyField<T> : PropertyField, IModelFieldOf<T>
	{
		private string _fieldName;
		public override string FieldName => _fieldName;

		private bool _canWrite;
		public override bool CanWrite => _canWrite;

		private bool _canRead;
		public override bool CanRead => _canRead;

		private DataType<T> _fieldType;
		public new DataType<T> FieldType => _fieldType;

		protected override void InitalizeFromPropertyInfoOverride(PropertyInfo propertyInfo)
		{
			_fieldName = propertyInfo.Name;
			_canRead = propertyInfo.CanRead;
			_canWrite = propertyInfo.CanWrite;
			_fieldType = DataType.GetTypeOf<T>();
		}

		protected override DataType GetDataType() => _fieldType;

		public override IModelNode Accept(IModelVisitor visitor)
		{
			return visitor.VisitField(this);
		}
	}
}
