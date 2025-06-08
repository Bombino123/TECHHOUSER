using System;
using System.Runtime.Serialization;

namespace dnlib.DotNet;

[Serializable]
public class MemberRefResolveException : ResolveException
{
	public MemberRefResolveException()
	{
	}

	public MemberRefResolveException(string message)
		: base(message)
	{
	}

	public MemberRefResolveException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected MemberRefResolveException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
