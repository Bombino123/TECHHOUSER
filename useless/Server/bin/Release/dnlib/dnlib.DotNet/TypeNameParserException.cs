using System;
using System.Runtime.Serialization;

namespace dnlib.DotNet;

[Serializable]
public class TypeNameParserException : Exception
{
	public TypeNameParserException()
	{
	}

	public TypeNameParserException(string message)
		: base(message)
	{
	}

	public TypeNameParserException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected TypeNameParserException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
