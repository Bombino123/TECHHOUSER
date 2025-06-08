using System.Data.Entity.Resources;
using System.Runtime.Serialization;

namespace System.Data.Entity.Core;

[Serializable]
public sealed class InvalidCommandTreeException : DataException
{
	public InvalidCommandTreeException()
		: base(Strings.Cqt_Exceptions_InvalidCommandTree)
	{
	}

	public InvalidCommandTreeException(string message)
		: base(message)
	{
	}

	public InvalidCommandTreeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private InvalidCommandTreeException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
