using System.Data.Entity.Core;
using System.Runtime.Serialization;

namespace System.Data.Entity.Infrastructure;

[Serializable]
public sealed class RetryLimitExceededException : EntityException
{
	public RetryLimitExceededException()
	{
	}

	public RetryLimitExceededException(string message)
		: base(message)
	{
	}

	public RetryLimitExceededException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private RetryLimitExceededException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
