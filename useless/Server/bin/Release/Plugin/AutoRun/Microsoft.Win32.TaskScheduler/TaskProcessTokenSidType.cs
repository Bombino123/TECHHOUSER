using System.Runtime.InteropServices;

namespace Microsoft.Win32.TaskScheduler;

[ComVisible(true)]
public enum TaskProcessTokenSidType
{
	None,
	Unrestricted,
	Default
}
