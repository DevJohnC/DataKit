using System.Collections.Generic;

namespace DataKit.SQL.Providers
{
	public class ParameterBag : Dictionary<string, object>
	{
		private int _autoParameterCount;

		public string Add(object value)
		{
			var name = $"autoPrmtr{++_autoParameterCount}";
			Add(name, value);
			return name;
		}

		public static ParameterBag Combine(params ParameterBag[] parameterBags)
		{
			var result = default(ParameterBag);
			foreach (var bag in parameterBags)
			{
				if (bag == null)
					continue;

				if (result == null)
					result = new ParameterBag();

				Copy(result, bag);
			}
			return result;
		}

		private static void Copy(ParameterBag into, ParameterBag from)
		{
			foreach (var kvp in from)
				into[kvp.Key] = kvp.Value;
		}
	}
}
