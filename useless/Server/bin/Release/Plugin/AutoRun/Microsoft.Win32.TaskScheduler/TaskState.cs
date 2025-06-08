using System.Runtime.InteropServices;

namespace Microsoft.Win32.TaskScheduler;

[ComVisible(true)]
public enum TaskState
{
	Unknown,
	Disabled,
	Queued,
	Ready,
	Running
}
