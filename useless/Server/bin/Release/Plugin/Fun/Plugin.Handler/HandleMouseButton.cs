namespace Plugin.Handler;

public class HandleMouseButton
{
	public void RestoreMouseButtons()
	{
		try
		{
			Native.SwapMouseButton(0);
		}
		catch
		{
		}
	}

	public void SwapMouseButtons()
	{
		try
		{
			Native.SwapMouseButton(1);
		}
		catch
		{
		}
	}
}
