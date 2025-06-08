using System.Runtime.Serialization;

namespace System.Data.Entity.Core;

[Serializable]
public sealed class EntityCommandCompilationException : EntityException
{
	private const int HResultCommandCompilation = -2146232005;

	public EntityCommandCompilationException()
	{
		base.HResult = -2146232005;
	}

	public EntityCommandCompilationException(string message)
		: base(message)
	{
		base.HResult = -2146232005;
	}

	public EntityCommandCompilationException(string message, Exception innerException)
		: base(message, innerException)
	{
		base.HResult = -2146232005;
	}

	private EntityCommandCompilationException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
		base.HResult = -2146232005;
	}
}
