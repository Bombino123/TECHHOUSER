using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace Plugin.Helper;

internal class Methods
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static GetThumbnailImageAbort _003C_003E9__8_0;

		internal bool _003CGetIcon_003Eb__8_0()
		{
			return false;
		}
	}

	public static string[] Disk()
	{
		List<string> list = new List<string>();
		DriveInfo[] drives = DriveInfo.GetDrives();
		foreach (DriveInfo driveInfo in drives)
		{
			string name = driveInfo.Name;
			if (driveInfo.DriveType == DriveType.Removable)
			{
				list.Add(name + ";Usb");
			}
			else
			{
				list.Add(name + ";Drive");
			}
		}
		return list.ToArray();
	}

	public static string GetVariablesDirs(string k)
	{
		Hashtable hashtable = (Hashtable)Environment.GetEnvironmentVariables();
		foreach (object key in hashtable.Keys)
		{
			if ((string)key == k)
			{
				return (string)hashtable[key];
			}
		}
		return "";
	}

	public static string[] GetVariablesDirs()
	{
		List<string> list = new List<string>();
		Hashtable hashtable = (Hashtable)Environment.GetEnvironmentVariables();
		foreach (object key in hashtable.Keys)
		{
			string text = (string)hashtable[key];
			if (!text.Contains(";") && !text.Contains(",") && !text.Contains(".") && text.Contains("C:\\"))
			{
				list.Add((string)key);
			}
		}
		return list.ToArray();
	}

	public static object[] GetIcons(string path)
	{
		List<object> list = new List<object>();
		string[] files = Directory.GetFiles(path);
		foreach (string file in files)
		{
			string hashCode = GetHashCode(GetIcon(file));
			bool flag = true;
			for (int j = 1; j < list.Count; j += 2)
			{
				if ((string)list[j] == hashCode)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				list.Add(GetIcon(file));
				list.Add(hashCode);
			}
		}
		return list.ToArray();
	}

	public static string GetHashCode(byte[] obj)
	{
		MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
		byte[] buffer = obj;
		buffer = mD5CryptoServiceProvider.ComputeHash(buffer);
		return ByteToStr(buffer);
	}

	private static string ByteToStr(byte[] buffer)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (byte b in buffer)
		{
			stringBuilder.Append(b.ToString("x2"));
		}
		return stringBuilder.ToString();
	}

	public static object[] GetFiles(string path)
	{
		List<object> list = new List<object>();
		string[] files = Directory.GetFiles(path);
		foreach (string text in files)
		{
			list.Add(Path.GetFileName(text));
			list.Add(GetHashCode(GetIcon(text)));
			list.Add(new FileInfo(text).Attributes.ToString());
			list.Add(new FileInfo(text).LastWriteTime.ToString());
			list.Add(new FileInfo(text).Length);
		}
		return list.ToArray();
	}

	public static object[] GetDirs(string path)
	{
		List<object> list = new List<object>();
		string[] directories = Directory.GetDirectories(path);
		foreach (string text in directories)
		{
			list.Add(Path.GetFileName(text));
			list.Add(new FileInfo(text).Attributes.ToString());
			list.Add(new FileInfo(text).LastWriteTime.ToString());
		}
		return list.ToArray();
	}

	public static byte[] GetIcon(string file)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		try
		{
			if (file.EndsWith("jpg") || file.EndsWith("jpeg") || file.EndsWith("gif") || file.EndsWith("png") || file.EndsWith("bmp"))
			{
				Bitmap val = new Bitmap(file);
				try
				{
					using MemoryStream memoryStream = new MemoryStream();
					object obj = _003C_003Ec._003C_003E9__8_0;
					if (obj == null)
					{
						GetThumbnailImageAbort val2 = () => false;
						_003C_003Ec._003C_003E9__8_0 = val2;
						obj = (object)val2;
					}
					((Image)new Bitmap(((Image)val).GetThumbnailImage(48, 48, (GetThumbnailImageAbort)obj, IntPtr.Zero))).Save((Stream)memoryStream, ImageFormat.Png);
					return memoryStream.ToArray();
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			Icon val3 = Icon.ExtractAssociatedIcon(file);
			try
			{
				using MemoryStream memoryStream2 = new MemoryStream();
				((Image)val3.ToBitmap()).Save((Stream)memoryStream2, ImageFormat.Png);
				return memoryStream2.ToArray();
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
		catch
		{
			using MemoryStream memoryStream3 = new MemoryStream();
			((Image)new Bitmap(48, 48)).Save((Stream)memoryStream3, ImageFormat.Png);
			return memoryStream3.ToArray();
		}
	}
}
