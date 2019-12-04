namespace DataKit.Modelling
{
	/// <summary>
	/// A field in a data structure model.
	/// </summary>
	public interface IModelField : IModelNode
	{
		/// <summary>
		/// Gets the field's data type.
		/// </summary>
		DataType FieldType { get; }

		/// <summary>
		/// Gets the field's name.
		/// </summary>
		string FieldName { get; }

		/// <summary>
		/// Gets a value indicating if the field can be written to.
		/// </summary>
		bool CanWrite { get; }

		/// <summary>
		/// Gets a value indicating if the field can be read from.
		/// </summary>
		bool CanRead { get; }

		/// <summary>
		/// Gets a model of the fields that reside beneath this node in the model graph.
		/// </summary>
		IDataModel FieldModel { get; }
	}

	public interface IModelField<TModel, TField> : IModelField
		where TModel : IDataModel<TModel, TField>
		where TField : IModelField<TModel, TField>
	{
		new TModel FieldModel { get; }
	}

	/// <summary>
	/// A field of type T in a data structure model.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IModelFieldOf<T> : IModelField
	{
		new DataType<T> FieldType { get; }
	}
}
