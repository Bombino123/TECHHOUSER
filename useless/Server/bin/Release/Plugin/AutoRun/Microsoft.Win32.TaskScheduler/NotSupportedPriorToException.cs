using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Microsoft.Win32.TaskScheduler;

[Serializable]
[DebuggerStepThrough]
[ComVisible(true)]
public class NotSupportedPriorToException : TSNotSupportedException
{
	internal override string LibName => $"Task Scheduler versions prior to 2.{(int)(min - 2)} (1.{(int)min})";

	protected NotSupportedPriorToException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}

	internal NotSupportedPriorToException(TaskCompatibility supportedVersion)
		: base(supportedVersion)
	{
	}
}
