namespace DataKit.ORM
{
	/// <summary>
	/// Represents an operation that will be executed on a data store.
	/// </summary>
	public abstract class DataOperation
	{
	}

	public abstract class DataOperation<TDataSet> : DataOperation
		where TDataSet : DataSet
	{
		public TDataSet DataSet { get; }

		protected DataOperation(TDataSet dataSet)
		{
			DataSet = dataSet;
		}
	}
}
