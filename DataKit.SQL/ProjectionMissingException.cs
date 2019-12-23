using System;
using System.Runtime.Serialization;

namespace DataKit.SQL
{
	[Serializable]
	internal class ProjectionMissingException : QueryRequirementException
	{
		public ProjectionMissingException()
		{
		}

		public ProjectionMissingException(string message) : base(message)
		{
		}

		public ProjectionMissingException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ProjectionMissingException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}