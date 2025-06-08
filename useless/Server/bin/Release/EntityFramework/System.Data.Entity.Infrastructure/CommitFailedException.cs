using System.Data.Entity.Resources;
using System.Runtime.Serialization;

namespace System.Data.Entity.Infrastructure;

[Serializable]
public class CommitFailedException : DataException
{
	public CommitFailedException()
		: base(Strings.CommitFailed)
	{
	}

	public CommitFailedException(string message)
		: base(message)
	{
	}

	public CommitFailedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected CommitFailedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
