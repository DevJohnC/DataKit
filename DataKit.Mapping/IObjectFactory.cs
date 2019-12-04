using System;

namespace DataKit.Mapping
{
	public interface IObjectFactory
	{
		object CreateInstance(Type type);
	}
}
