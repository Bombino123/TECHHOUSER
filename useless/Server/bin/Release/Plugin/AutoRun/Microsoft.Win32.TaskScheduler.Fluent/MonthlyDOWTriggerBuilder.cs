using System.Runtime.InteropServices;

namespace Microsoft.Win32.TaskScheduler.Fluent;

[ComVisible(true)]
public class MonthlyDOWTriggerBuilder : BaseBuilder
{
	private TriggerBuilder trb;

	internal MonthlyDOWTriggerBuilder(BuilderInfo taskBuilder, DaysOfTheWeek dow)
		: base(taskBuilder)
	{
		trb = new TriggerBuilder(taskBuilder, dow);
	}

	public MonthlyDOWTriggerBuilder In(WhichWeek ww)
	{
		((MonthlyDOWTrigger)trb.trigger).WeeksOfMonth = ww;
		return this;
	}

	public TriggerBuilder Of(MonthsOfTheYear moy)
	{
		((MonthlyDOWTrigger)trb.trigger).MonthsOfYear = moy;
		return trb;
	}
}
