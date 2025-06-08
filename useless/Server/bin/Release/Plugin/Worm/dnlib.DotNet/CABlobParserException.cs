using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace dnlib.DotNet;

[Serializable]
[ComVisible(true)]
public class CABlobParserException : Exception
{
	public CABlobParserException()
	{
	}

	public CABlobParserException(string message)
		: base(message)
	{
	}

	public CABlobParserException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected CABlobParserException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
