using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Plugin.Helper;

public class Wallpaper
{
	public static readonly uint SPI_SETDESKWALLPAPER = 20u;

	public static readonly uint SPIF_UPDATEINIFILE = 1u;

	public static readonly uint SPIF_SENDWININICHANGE = 2u;

	[DllImport("user32.dll")]
	public static extern uint SystemParametersInfo(uint action, uint uParam, string vParam, uint winIni);

	public static void Change(string filepath)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		Bitmap val = new Bitmap(filepath);
		try
		{
			Graphics val2 = Graphics.FromImage((Image)(object)val);
			try
			{
				((Image)val).Save(filepath, ImageFormat.Bmp);
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
		SystemParametersInfo(SPI_SETDESKWALLPAPER, 0u, filepath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
	}
}
