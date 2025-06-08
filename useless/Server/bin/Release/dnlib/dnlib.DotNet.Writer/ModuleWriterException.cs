using System;
using System.Runtime.Serialization;

namespace dnlib.DotNet.Writer;

[Serializable]
public class ModuleWriterException : Exception
{
	public ModuleWriterException()
	{
	}

	public ModuleWriterException(string message)
		: base(message)
	{
	}

	public ModuleWriterException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected ModuleWriterException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
