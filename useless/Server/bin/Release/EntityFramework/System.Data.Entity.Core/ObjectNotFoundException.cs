using System.Runtime.Serialization;

namespace System.Data.Entity.Core;

[Serializable]
public sealed class ObjectNotFoundException : DataException
{
	public ObjectNotFoundException()
	{
	}

	public ObjectNotFoundException(string message)
		: base(message)
	{
	}

	public ObjectNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private ObjectNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
