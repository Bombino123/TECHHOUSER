using System;

namespace AntdUI;

public class VEventArgs<T> : EventArgs
{
	public T Value { get; private set; }

	public VEventArgs(T value)
	{
		Value = value;
	}
}
