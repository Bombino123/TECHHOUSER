using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.TaskScheduler;

[DefaultValue(TaskInstancesPolicy.IgnoreNew)]
[ComVisible(true)]
public enum TaskInstancesPolicy
{
	Parallel,
	Queue,
	IgnoreNew,
	StopExisting
}
