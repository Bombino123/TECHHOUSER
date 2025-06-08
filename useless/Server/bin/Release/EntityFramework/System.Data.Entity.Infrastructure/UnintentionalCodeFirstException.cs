using System.Data.Entity.Resources;
using System.Runtime.Serialization;

namespace System.Data.Entity.Infrastructure;

[Serializable]
public class UnintentionalCodeFirstException : InvalidOperationException
{
	public UnintentionalCodeFirstException()
		: base(Strings.UnintentionalCodeFirstException_Message)
	{
	}

	protected UnintentionalCodeFirstException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public UnintentionalCodeFirstException(string message)
		: base(message)
	{
	}

	public UnintentionalCodeFirstException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
