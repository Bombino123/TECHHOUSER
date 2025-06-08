using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace dnlib.DotNet.Writer;

[Serializable]
[ComVisible(true)]
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
