using System.Runtime.InteropServices;

namespace Microsoft.Win32.TaskScheduler.Fluent;

[ComVisible(true)]
public class WeeklyTriggerBuilder : TriggerBuilder
{
	internal WeeklyTriggerBuilder(BuilderInfo taskBuilder, short interval)
		: base(taskBuilder)
	{
		TriggerCollection triggers = base.TaskDef.Triggers;
		WeeklyTrigger obj = new WeeklyTrigger(DaysOfTheWeek.Sunday, 1)
		{
			WeeksInterval = interval
		};
		Trigger unboundTrigger = obj;
		trigger = obj;
		triggers.Add(unboundTrigger);
	}

	public TriggerBuilder On(DaysOfTheWeek dow)
	{
		((WeeklyTrigger)trigger).DaysOfWeek = dow;
		return this;
	}
}
