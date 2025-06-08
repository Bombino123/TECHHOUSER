using System.Data.Entity.Resources;
using System.Runtime.Serialization;

namespace System.Data.Entity.Core;

[Serializable]
public class EntityException : DataException
{
	public EntityException()
		: base(Strings.EntityClient_ProviderGeneralError)
	{
	}

	public EntityException(string message)
		: base(message)
	{
	}

	public EntityException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected EntityException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
