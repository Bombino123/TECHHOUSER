using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Microsoft.Win32.TaskScheduler.Fluent;

[ComVisible(true)]
public class TriggerBuilder : BaseBuilder
{
	internal Trigger trigger;

	internal TriggerBuilder(BuilderInfo taskBuilder)
		: base(taskBuilder)
	{
	}

	internal TriggerBuilder(BuilderInfo taskBuilder, DaysOfTheWeek dow)
		: this(taskBuilder)
	{
		base.TaskDef.Triggers.Add(trigger = new MonthlyDOWTrigger(dow));
	}

	internal TriggerBuilder(BuilderInfo taskBuilder, MonthsOfTheYear moy)
		: this(taskBuilder)
	{
		TriggerCollection triggers = base.TaskDef.Triggers;
		MonthlyTrigger obj = new MonthlyTrigger
		{
			MonthsOfYear = moy
		};
		Trigger unboundTrigger = obj;
		trigger = obj;
		triggers.Add(unboundTrigger);
	}

	internal TriggerBuilder(BuilderInfo taskBuilder, TaskTriggerType taskTriggerType)
		: this(taskBuilder)
	{
		base.TaskDef.Triggers.Add(trigger = Trigger.CreateTrigger(taskTriggerType));
	}

	public TriggerBuilder Ending(int year, int month, int day)
	{
		trigger.EndBoundary = new DateTime(year, month, day, trigger.StartBoundary.Hour, trigger.StartBoundary.Minute, trigger.StartBoundary.Second);
		return this;
	}

	public TriggerBuilder Ending(int year, int month, int day, int hour, int min, int sec)
	{
		trigger.EndBoundary = new DateTime(year, month, day, hour, min, sec);
		return this;
	}

	public TriggerBuilder Ending([NotNull] string dt)
	{
		trigger.EndBoundary = DateTime.Parse(dt);
		return this;
	}

	public TriggerBuilder Ending(DateTime dt)
	{
		trigger.EndBoundary = dt;
		return this;
	}

	public TriggerBuilder IsDisabled()
	{
		trigger.Enabled = false;
		return this;
	}

	public TriggerBuilder RepeatingEvery(TimeSpan span)
	{
		trigger.Repetition.Interval = span;
		return this;
	}

	public TriggerBuilder RepeatingEvery([NotNull] string span)
	{
		trigger.Repetition.Interval = TimeSpan.Parse(span);
		return this;
	}

	public TriggerBuilder RunningAtMostFor(TimeSpan span)
	{
		trigger.Repetition.Duration = span;
		return this;
	}

	public TriggerBuilder RunningAtMostFor([NotNull] string span)
	{
		trigger.Repetition.Duration = TimeSpan.Parse(span);
		return this;
	}

	public TriggerBuilder Starting(int year, int month, int day)
	{
		trigger.StartBoundary = new DateTime(year, month, day, trigger.StartBoundary.Hour, trigger.StartBoundary.Minute, trigger.StartBoundary.Second);
		return this;
	}

	public TriggerBuilder Starting(int year, int month, int day, int hour, int min, int sec)
	{
		trigger.StartBoundary = new DateTime(year, month, day, hour, min, sec);
		return this;
	}

	public TriggerBuilder Starting([NotNull] string dt)
	{
		trigger.StartBoundary = DateTime.Parse(dt);
		return this;
	}

	public TriggerBuilder Starting(DateTime dt)
	{
		trigger.StartBoundary = dt;
		return this;
	}
}
