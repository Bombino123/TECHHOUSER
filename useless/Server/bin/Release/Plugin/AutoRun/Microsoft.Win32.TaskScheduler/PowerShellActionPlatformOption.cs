using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.TaskScheduler;

[Flags]
[ComVisible(true)]
public enum PowerShellActionPlatformOption
{
	Never = 0,
	Version1 = 1,
	Version2 = 2,
	All = 3
}
