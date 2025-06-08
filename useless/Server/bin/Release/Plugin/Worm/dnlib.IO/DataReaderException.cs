using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace dnlib.IO;

[Serializable]
[ComVisible(true)]
public sealed class DataReaderException : IOException
{
	internal DataReaderException(string message)
		: base(message)
	{
	}

	internal DataReaderException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
