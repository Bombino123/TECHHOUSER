using System.Data.Entity.Resources;
using System.Runtime.Serialization;

namespace System.Data.Entity.Core;

[Serializable]
public sealed class MappingException : EntityException
{
	public MappingException()
		: base(Strings.Mapping_General_Error)
	{
	}

	public MappingException(string message)
		: base(message)
	{
	}

	public MappingException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private MappingException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
