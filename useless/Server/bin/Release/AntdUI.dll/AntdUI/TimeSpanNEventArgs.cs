using System;

namespace AntdUI;

public class TimeSpanNEventArgs : VEventArgs<TimeSpan>
{
	public TimeSpanNEventArgs(TimeSpan value)
		: base(value)
	{
	}
}
