using System.Runtime.InteropServices;

namespace Microsoft.Win32.TaskScheduler;

[ComVisible(true)]
public enum TaskActionType
{
	Execute = 0,
	ComHandler = 5,
	SendEmail = 6,
	ShowMessage = 7
}
