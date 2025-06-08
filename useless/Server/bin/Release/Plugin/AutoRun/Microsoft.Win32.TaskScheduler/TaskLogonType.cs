using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.TaskScheduler;

[DefaultValue(TaskLogonType.S4U)]
[ComVisible(true)]
public enum TaskLogonType
{
	None,
	Password,
	S4U,
	InteractiveToken,
	Group,
	ServiceAccount,
	InteractiveTokenOrPassword
}
