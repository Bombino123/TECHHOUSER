using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Microsoft.Win32.TaskScheduler.Fluent;

[ComVisible(true)]
public class MonthlyTriggerBuilder : BaseBuilder
{
	private TriggerBuilder trb;

	internal MonthlyTriggerBuilder(BuilderInfo taskBuilder, MonthsOfTheYear moy)
		: base(taskBuilder)
	{
		trb = new TriggerBuilder(taskBuilder, moy);
	}

	public TriggerBuilder OnTheDays([NotNull] params int[] days)
	{
		((MonthlyTrigger)trb.trigger).DaysOfMonth = days;
		return trb;
	}
}
