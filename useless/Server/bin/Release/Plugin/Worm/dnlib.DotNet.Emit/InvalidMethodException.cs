using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace dnlib.DotNet.Emit;

[Serializable]
[ComVisible(true)]
public class InvalidMethodException : Exception
{
	public InvalidMethodException()
	{
	}

	public InvalidMethodException(string msg)
		: base(msg)
	{
	}

	public InvalidMethodException(string msg, Exception innerException)
		: base(msg, innerException)
	{
	}

	protected InvalidMethodException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
