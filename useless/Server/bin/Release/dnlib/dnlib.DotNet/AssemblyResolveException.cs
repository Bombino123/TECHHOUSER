using System;
using System.Runtime.Serialization;

namespace dnlib.DotNet;

[Serializable]
public class AssemblyResolveException : ResolveException
{
	public AssemblyResolveException()
	{
	}

	public AssemblyResolveException(string message)
		: base(message)
	{
	}

	public AssemblyResolveException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected AssemblyResolveException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
