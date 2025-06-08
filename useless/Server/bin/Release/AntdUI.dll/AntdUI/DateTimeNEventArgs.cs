using System;

namespace AntdUI;

public class DateTimeNEventArgs : VEventArgs<DateTime?>
{
	public DateTimeNEventArgs(DateTime? value)
		: base(value)
	{
	}
}
