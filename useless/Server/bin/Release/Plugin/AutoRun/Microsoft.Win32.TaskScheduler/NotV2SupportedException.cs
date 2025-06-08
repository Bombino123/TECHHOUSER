using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Microsoft.Win32.TaskScheduler;

[Serializable]
[DebuggerStepThrough]
[ComVisible(true)]
public class NotV2SupportedException : TSNotSupportedException
{
	internal override string LibName => "Task Scheduler 2.0 (1.2)";

	protected NotV2SupportedException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}

	internal NotV2SupportedException()
		: base(TaskCompatibility.V1)
	{
	}

	internal NotV2SupportedException(string message)
		: base(message, TaskCompatibility.V1)
	{
	}
}
