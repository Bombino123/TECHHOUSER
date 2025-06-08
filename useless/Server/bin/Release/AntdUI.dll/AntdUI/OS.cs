using System;
using System.Runtime.InteropServices;

namespace AntdUI;

public class OS
{
	internal struct OSVERSIONINFOEX
	{
		internal int OSVersionInfoSize;

		internal int MajorVersion;

		internal int MinorVersion;

		internal int BuildNumber;

		internal int PlatformId;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		internal string CSDVersion;

		internal ushort ServicePackMajor;

		internal ushort ServicePackMinor;

		internal short SuiteMask;

		internal byte ProductType;

		internal byte Reserved;
	}

	public static Version Version;

	public static bool Win11
	{
		get
		{
			Version version = Version;
			if (version.Major >= 10 && version.Build > 22000)
			{
				return true;
			}
			return false;
		}
	}

	static OS()
	{
		try
		{
			OSVERSIONINFOEX versionInfo = new OSVERSIONINFOEX
			{
				OSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX))
			};
			if (RtlGetVersion(ref versionInfo) == 0)
			{
				Version = new Version(versionInfo.MajorVersion, versionInfo.MinorVersion, versionInfo.BuildNumber);
				return;
			}
		}
		catch
		{
		}
		Version = Environment.OSVersion.Version;
	}

	[DllImport("ntdll.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	internal static extern int RtlGetVersion(ref OSVERSIONINFOEX versionInfo);
}
