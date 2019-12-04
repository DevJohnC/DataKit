using System.Runtime.CompilerServices;

namespace DataKit.Mapping.Binding
{
	public class BindingContext
	{
		private readonly ConditionalWeakTable<object, object> _cache = new ConditionalWeakTable<object, object>();

		public bool TryGetCachedValue(object key, out object cachedValue)
			=> _cache.TryGetValue(key, out cachedValue);

		public void AddCachedValue(object key, object value)
		{
			_cache.Add(key, value);
		}
	}
}
