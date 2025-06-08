using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.TaskScheduler;

[ComVisible(true)]
public static class TaskServiceVersion
{
	[Description("Task Scheduler 1.0 (Windows Server™ 2003, Windows® XP, or Windows® 2000).")]
	public static readonly Version V1_1 = new Version(1, 1);

	[Description("Task Scheduler 2.0 (Windows Vista™, Windows Server™ 2008).")]
	public static readonly Version V1_2 = new Version(1, 2);

	[Description("Task Scheduler 2.1 (Windows® 7, Windows Server™ 2008 R2).")]
	public static readonly Version V1_3 = new Version(1, 3);

	[Description("Task Scheduler 2.2 (Windows® 8.x, Windows Server™ 2012).")]
	public static readonly Version V1_4 = new Version(1, 4);

	[Description("Task Scheduler 2.3 (Windows® 10, Windows Server™ 2016).")]
	public static readonly Version V1_5 = new Version(1, 5);

	[Description("Task Scheduler 2.3 (Windows® 10, Windows Server™ 2016 post build 1703).")]
	public static readonly Version V1_6 = new Version(1, 6);
}
