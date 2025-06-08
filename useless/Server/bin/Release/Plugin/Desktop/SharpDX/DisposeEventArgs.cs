using System;

namespace SharpDX;

public class DisposeEventArgs : EventArgs
{
	public static readonly DisposeEventArgs DisposingEventArgs = new DisposeEventArgs(disposing: true);

	public static readonly DisposeEventArgs NotDisposingEventArgs = new DisposeEventArgs(disposing: false);

	public readonly bool Disposing;

	private DisposeEventArgs(bool disposing)
	{
		Disposing = disposing;
	}

	public static DisposeEventArgs Get(bool disposing)
	{
		if (!disposing)
		{
			return NotDisposingEventArgs;
		}
		return DisposingEventArgs;
	}
}
