using DataKit.Modelling;

namespace DataKit.Mapping
{
	public interface IDataModelReader
	{
	}

	public interface IDataModelReader<TDataModel> : IDataModelReader
		where TDataModel : IDataModel
	{
		TDataModel Model { get; }
	}

	public interface IDataModelReader<TDataModel, TField> : IDataModelReader<TDataModel>
		where TDataModel : IDataModel<TField>
		where TField : IModelField
	{
		T ReadField<T>(TField field);

		void EnterMember(TField field);
		void LeaveMember();

		void EnterEnumerable();
		void LeaveEnumerable();
		bool MoveNext();
	}
}
