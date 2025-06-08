using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace dnlib.DotNet;

[Serializable]
[ComVisible(true)]
public class InvalidKeyException : Exception
{
	public InvalidKeyException()
	{
	}

	public InvalidKeyException(string message)
		: base(message)
	{
	}

	public InvalidKeyException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected InvalidKeyException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
