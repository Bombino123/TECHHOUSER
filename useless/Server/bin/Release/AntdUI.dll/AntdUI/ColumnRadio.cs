using System;

namespace AntdUI;

public class ColumnRadio : Column
{
	public bool AutoCheck { get; set; } = true;


	public Func<bool, object?, int, int, bool>? Call { get; set; }

	public new Func<object?, object, int, object?>? Render { get; }

	public ColumnRadio(string key, string title)
		: base(key, title)
	{
		base.Align = ColumnAlign.Center;
	}
}
