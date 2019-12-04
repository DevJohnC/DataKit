namespace DataKit.ORM
{
	/// <summary>
	/// Utility class for accessing various server functions.
	/// </summary>
	/// <remarks>Helpful if you don't have access to functions on an object instance.</remarks>
	public static class ServerFunctions
	{
		public static SqlServerFunctions SqlServerFunctions { get; }
	}
}
