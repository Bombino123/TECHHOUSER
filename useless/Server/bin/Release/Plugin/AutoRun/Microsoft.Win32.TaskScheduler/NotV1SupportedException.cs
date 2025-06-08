using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Microsoft.Win32.TaskScheduler;

[Serializable]
[DebuggerStepThrough]
[ComVisible(true)]
public class NotV1SupportedException : TSNotSupportedException
{
	internal override string LibName => "Task Scheduler 1.0";

	protected NotV1SupportedException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}

	internal NotV1SupportedException()
		: base(TaskCompatibility.V2)
	{
	}

	public NotV1SupportedException(string message)
		: base(message, TaskCompatibility.V2)
	{
	}
}
