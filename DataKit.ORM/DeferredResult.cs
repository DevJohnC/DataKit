namespace DataKit.ORM
{
	public class DeferredResult<T>
	{
		public T Result { get; internal set; }
	}

	public class DeferredResultSource<T>
	{
		public DeferredResult<T> Result { get; }

		public DeferredResultSource()
		{
			Result = new DeferredResult<T>();
		}

		public void SetResult(T value)
		{
			Result.Result = value;
		}
	}
}
