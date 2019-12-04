using System.Collections.Generic;

namespace DataKit.Modelling
{
	/// <summary>
	/// A model of a data structure.
	/// </summary>
	public interface IDataModel : IModelNode
	{
		IReadOnlyList<IModelField> Fields { get; }

		IModelField this[string fieldName] { get; }

		bool TryGetField(string fieldName, out IModelField field);
	}

	public interface IDataModel<TField> : IDataModel
		where TField : IModelField
	{
		new IReadOnlyList<TField> Fields { get; }

		new TField this[string fieldName] { get; }

		bool TryGetField(string fieldName, out TField field);
	}

	/// <summary>
	/// A model of a data structure.
	/// </summary>
	/// <typeparam name="TField"></typeparam>
	public interface IDataModel<TModel, TField> : IDataModel<TField>
		where TModel : IDataModel<TModel, TField>
		where TField : IModelField<TModel, TField>
	{
		new IReadOnlyList<TField> Fields { get; }

		new TField this[string fieldName] { get; }

		new bool TryGetField(string fieldName, out TField field);
	}
}
