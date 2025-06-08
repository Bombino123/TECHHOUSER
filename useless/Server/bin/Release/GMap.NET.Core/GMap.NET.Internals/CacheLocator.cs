#define TRACE
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;

namespace GMap.NET.Internals;

internal class CacheLocator
{
	private static string _location;

	public static bool Delay;

	public static string Location
	{
		get
		{
			if (string.IsNullOrEmpty(_location))
			{
				Reset();
			}
			return _location;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				Reset();
			}
			else
			{
				_location = value;
			}
			if (Delay)
			{
				Cache.Instance.CacheLocation = _location;
			}
		}
	}

	private static void Reset()
	{
		string applicationDataFolderPath = GetApplicationDataFolderPath();
		if (string.IsNullOrEmpty(applicationDataFolderPath))
		{
			GMaps.Instance.Mode = AccessMode.ServerOnly;
			GMaps.Instance.UseDirectionsCache = false;
			GMaps.Instance.UseGeocoderCache = false;
			GMaps.Instance.UsePlacemarkCache = false;
			GMaps.Instance.UseRouteCache = false;
			GMaps.Instance.UseUrlCache = false;
		}
		else
		{
			Location = applicationDataFolderPath;
		}
	}

	public static string GetApplicationDataFolderPath()
	{
		bool flag = false;
		try
		{
			using WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
			if (windowsIdentity != null)
			{
				flag = windowsIdentity.IsSystem;
			}
		}
		catch (Exception ex)
		{
			Trace.WriteLine("SQLitePureImageCache, WindowsIdentity.GetCurrent: " + ex);
		}
		string text = ((!flag) ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) : Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
		if (!string.IsNullOrEmpty(text))
		{
			string text2 = text;
			char directorySeparatorChar = Path.DirectorySeparatorChar;
			string text3 = directorySeparatorChar.ToString();
			directorySeparatorChar = Path.DirectorySeparatorChar;
			text = text2 + text3 + "GMap.NET" + directorySeparatorChar;
		}
		return text;
	}
}
