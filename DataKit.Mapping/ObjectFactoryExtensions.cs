namespace DataKit.Mapping
{
	public static class ObjectFactoryExtensions
	{
		public static T CreateInstance<T>(this IObjectFactory factory)
		{
			return (T)factory.CreateInstance(typeof(T));
		}
	}
}
