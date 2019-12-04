using System;
using System.Collections.Generic;

namespace DataKit.Modelling
{
	public class CachedCollection<TKey, TValue>
	{
		private readonly object _syncronized = new object();

		private readonly Dictionary<TKey, TValue> _cacheStore = new Dictionary<TKey, TValue>();

		public bool TryGetValue(TKey key, out TValue value)
			=> _cacheStore.TryGetValue(key, out value);

		public TValue CreateIfNeeded(TKey key, Func<TValue> factory)
		{
			lock (_syncronized)
			{
				if (TryGetValue(key, out var value))
					return value;

				value = factory();
				_cacheStore.Add(key, value);
				return value;
			}
		}
	}

	public class CachedCollection<TKey, TFactoryArg, TValue> : CachedCollection<TKey, TValue>
	{
		private readonly Func<TFactoryArg, TValue> _factory;

		public CachedCollection(Func<TFactoryArg, TValue> factory)
		{
			_factory = factory;
		}

		public TValue GetOrCreate(TKey key, TFactoryArg factoryArg)
		{
			if (TryGetValue(key, out var value))
				return value;

			return CreateIfNeeded(key, () => _factory(factoryArg));
		}
	}
}
