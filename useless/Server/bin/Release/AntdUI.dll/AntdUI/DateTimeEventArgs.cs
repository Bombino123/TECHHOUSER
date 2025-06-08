using System;

namespace AntdUI;

public class DateTimeEventArgs : VEventArgs<DateTime>
{
	public DateTimeEventArgs(DateTime value)
		: base(value)
	{
	}
}
