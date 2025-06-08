using System.Runtime.InteropServices;

namespace Microsoft.Win32.TaskScheduler.Fluent;

[ComVisible(true)]
public class IntervalTriggerBuilder : BaseBuilder
{
	internal short interval;

	internal IntervalTriggerBuilder(BuilderInfo taskBuilder, short interval)
		: base(taskBuilder)
	{
		this.interval = interval;
	}

	public TriggerBuilder Days()
	{
		return new TriggerBuilder(tb)
		{
			trigger = base.TaskDef.Triggers.Add(new DailyTrigger(interval))
		};
	}

	public WeeklyTriggerBuilder Weeks()
	{
		return new WeeklyTriggerBuilder(tb, interval);
	}
}
