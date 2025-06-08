using System;

namespace AntdUI;

public class ColumnSwitch : Column
{
	public bool AutoCheck { get; set; } = true;


	public Func<bool, object?, int, int, bool>? Call { get; set; }

	public new Func<object?, object, int, object?>? Render { get; }

	public ColumnSwitch(string key, string title)
		: base(key, title)
	{
	}

	public ColumnSwitch(string key, string title, ColumnAlign align)
		: base(key, title, align)
	{
	}
}
