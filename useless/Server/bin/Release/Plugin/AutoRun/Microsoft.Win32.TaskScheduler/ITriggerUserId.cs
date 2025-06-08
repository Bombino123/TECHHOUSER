using System.Runtime.InteropServices;

namespace Microsoft.Win32.TaskScheduler;

[ComVisible(true)]
public interface ITriggerUserId
{
	string UserId { get; set; }
}
