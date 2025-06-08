using System.Runtime.Serialization;

namespace System.Data.Entity.Core;

[Serializable]
public sealed class EntityCommandExecutionException : EntityException
{
	private const int HResultCommandExecution = -2146232004;

	public EntityCommandExecutionException()
	{
		base.HResult = -2146232004;
	}

	public EntityCommandExecutionException(string message)
		: base(message)
	{
		base.HResult = -2146232004;
	}

	public EntityCommandExecutionException(string message, Exception innerException)
		: base(message, innerException)
	{
		base.HResult = -2146232004;
	}

	private EntityCommandExecutionException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
		base.HResult = -2146232004;
	}
}
