using System;
using System.Runtime.Serialization;

namespace dnlib.DotNet.Resources;

[Serializable]
public sealed class ResourceReaderException : Exception
{
	public ResourceReaderException()
	{
	}

	public ResourceReaderException(string msg)
		: base(msg)
	{
	}

	public ResourceReaderException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
