using System;
using System.Runtime.Serialization;

namespace DataKit.SQL
{
	[Serializable]
	internal class QueryRequirementException : Exception
	{
		public QueryRequirementException()
		{
		}

		public QueryRequirementException(string message) : base(message)
		{
		}

		public QueryRequirementException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected QueryRequirementException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}