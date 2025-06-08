using System;

namespace AntdUI;

public class PagePageEventArgs : EventArgs
{
	public int Current { get; private set; }

	public int Total { get; private set; }

	public int PageSize { get; private set; }

	public int PageTotal { get; private set; }

	public PagePageEventArgs(int current, int total, int pageSize, int pageTotal)
	{
		Current = current;
		Total = total;
		PageSize = pageSize;
		PageTotal = pageTotal;
	}
}
