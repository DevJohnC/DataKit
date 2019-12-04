using DataKit.Modelling;

namespace DataKit.Mapping
{
	public interface IDataModelWriter
	{
	}

	public interface IDataModelWriter<TDataModel> : IDataModelWriter
		where TDataModel : IDataModel
	{
		TDataModel Model { get; }
	}

	public interface IDataModelWriter<TDataModel, TField> : IDataModelWriter<TDataModel>
		where TDataModel : IDataModel<TField>
		where TField : IModelField
	{
		void WriteField<T>(TField field, T value);

		void EnterMember(TField field);
		void LeaveMember();

		void EnterEnumerable();
		void LeaveEnumerable();
		void MoveNext();
	}
}
