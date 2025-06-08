using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.TaskScheduler;

[Flags]
[ComVisible(true)]
public enum TaskSetSecurityOptions
{
	None = 0,
	DontAddPrincipalAce = 0x10
}
