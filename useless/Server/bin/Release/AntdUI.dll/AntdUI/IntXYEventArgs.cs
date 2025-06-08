using System;

namespace AntdUI;

public class IntXYEventArgs : EventArgs
{
	public int X { get; private set; }

	public int Y { get; private set; }

	public IntXYEventArgs(int x, int y)
	{
		X = x;
		Y = y;
	}
}
