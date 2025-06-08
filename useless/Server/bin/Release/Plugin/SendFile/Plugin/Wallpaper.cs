using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Plugin;

public class Wallpaper
{
	public static readonly uint SPI_SETDESKWALLPAPER = 20u;

	public static readonly uint SPIF_UPDATEINIFILE = 1u;

	public static readonly uint SPIF_SENDWININICHANGE = 2u;

	[DllImport("user32.dll")]
	public static extern uint SystemParametersInfo(uint action, uint uParam, string vParam, uint winIni);

	public void Change(byte[] img, string exe)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		string text = Path.Combine(Path.GetTempFileName() + exe);
		string text2 = Path.Combine(Path.GetTempFileName() + exe);
		File.WriteAllBytes(text, img);
		Bitmap val = new Bitmap(text);
		try
		{
			Graphics val2 = Graphics.FromImage((Image)(object)val);
			try
			{
				((Image)val).Save(text2, ImageFormat.Bmp);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", writable: true))
		{
			registryKey.SetValue("WallpaperStyle", 2.ToString());
			registryKey.SetValue("TileWallpaper", 0.ToString());
		}
		SystemParametersInfo(SPI_SETDESKWALLPAPER, 0u, text2, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
	}
}
