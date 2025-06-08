using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.TaskScheduler;

[ComVisible(true)]
public interface ITriggerDelay
{
	TimeSpan Delay { get; set; }
}
