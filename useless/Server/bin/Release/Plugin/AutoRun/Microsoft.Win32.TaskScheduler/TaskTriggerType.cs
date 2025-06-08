using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.TaskScheduler;

[DefaultValue(TaskTriggerType.Time)]
[ComVisible(true)]
public enum TaskTriggerType
{
	Event = 0,
	Time = 1,
	Daily = 2,
	Weekly = 3,
	Monthly = 4,
	MonthlyDOW = 5,
	Idle = 6,
	Registration = 7,
	Boot = 8,
	Logon = 9,
	SessionStateChange = 11,
	Custom = 12
}
