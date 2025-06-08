using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace dnlib.DotNet.Resources;

[Serializable]
[ComVisible(true)]
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
