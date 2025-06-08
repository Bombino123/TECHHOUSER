using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.TaskScheduler;

[Flags]
[ComVisible(true)]
public enum TaskRunFlags
{
	NoFlags = 0,
	AsSelf = 1,
	IgnoreConstraints = 2,
	UseSessionId = 4,
	UserSID = 8
}
