using System.Runtime.Serialization;

namespace System.Data.Entity.Core;

[Serializable]
public sealed class ProviderIncompatibleException : EntityException
{
	public ProviderIncompatibleException()
	{
	}

	public ProviderIncompatibleException(string message)
		: base(message)
	{
	}

	public ProviderIncompatibleException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private ProviderIncompatibleException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
