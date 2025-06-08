using System;
using System.Runtime.Serialization;

namespace dnlib.DotNet;

[Serializable]
public class TypeResolveException : ResolveException
{
	public TypeResolveException()
	{
	}

	public TypeResolveException(string message)
		: base(message)
	{
	}

	public TypeResolveException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected TypeResolveException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
