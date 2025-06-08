using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Microsoft.Win32.TaskScheduler.Fluent;

[ComVisible(true)]
public class ActionBuilder : BaseBuilder
{
	internal ActionBuilder(BuilderInfo taskBuilder, string path)
		: base(taskBuilder)
	{
		base.TaskDef.Actions.Add(new ExecAction(path));
	}

	public TriggerBuilder AtLogon()
	{
		return new TriggerBuilder(tb, TaskTriggerType.Logon);
	}

	public TriggerBuilder AtLogonOf(string userId)
	{
		TriggerBuilder triggerBuilder = new TriggerBuilder(tb, TaskTriggerType.Logon);
		((LogonTrigger)triggerBuilder.trigger).UserId = userId;
		return triggerBuilder;
	}

	public TriggerBuilder AtTaskRegistration()
	{
		return new TriggerBuilder(tb, TaskTriggerType.Registration);
	}

	public IntervalTriggerBuilder Every(short num)
	{
		return new IntervalTriggerBuilder(tb, num);
	}

	public MonthlyTriggerBuilder InTheMonthOf(MonthsOfTheYear moy)
	{
		return new MonthlyTriggerBuilder(tb, moy);
	}

	public ActionBuilder InWorkingDirectory([NotNull] string dir)
	{
		((ExecAction)base.TaskDef.Actions[0]).WorkingDirectory = dir;
		return this;
	}

	public MonthlyDOWTriggerBuilder OnAll(DaysOfTheWeek dow)
	{
		return new MonthlyDOWTriggerBuilder(tb, dow);
	}

	public TriggerBuilder OnBoot()
	{
		return new TriggerBuilder(tb, TaskTriggerType.Boot);
	}

	public TriggerBuilder Once()
	{
		return new TriggerBuilder(tb, TaskTriggerType.Time);
	}

	public TriggerBuilder OnIdle()
	{
		return new TriggerBuilder(tb, TaskTriggerType.Idle);
	}

	public TriggerBuilder OnStateChange(TaskSessionStateChangeType changeType)
	{
		TriggerBuilder triggerBuilder = new TriggerBuilder(tb, TaskTriggerType.SessionStateChange);
		((SessionStateChangeTrigger)triggerBuilder.trigger).StateChange = changeType;
		return triggerBuilder;
	}

	public ActionBuilder WithArguments([NotNull] string args)
	{
		((ExecAction)base.TaskDef.Actions[0]).Arguments = args;
		return this;
	}
}
