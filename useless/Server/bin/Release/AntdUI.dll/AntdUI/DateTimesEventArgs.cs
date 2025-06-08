using System;

namespace AntdUI;

public class DateTimesEventArgs : VEventArgs<DateTime[]?>
{
	public DateTimesEventArgs(DateTime[]? value)
		: base(value)
	{
	}
}
