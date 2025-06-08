using System.Runtime.InteropServices;

namespace Microsoft.Win32.TaskScheduler;

[ComVisible(true)]
public enum QuickTriggerType
{
	Boot,
	Idle,
	Logon,
	TaskRegistration,
	Hourly,
	Daily,
	Weekly,
	Monthly
}
