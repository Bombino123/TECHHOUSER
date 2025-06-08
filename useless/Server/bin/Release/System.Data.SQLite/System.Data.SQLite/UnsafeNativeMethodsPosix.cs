using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace System.Data.SQLite;

[SuppressUnmanagedCodeSecurity]
internal static class UnsafeNativeMethodsPosix
{
	internal sealed class utsname
	{
		public string sysname;

		public string nodename;

		public string release;

		public string version;

		public string machine;
	}

	private struct utsname_interop
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4096)]
		public byte[] buffer;
	}

	internal const int RTLD_LAZY = 1;

	internal const int RTLD_NOW = 2;

	internal const int RTLD_GLOBAL = 256;

	internal const int RTLD_LOCAL = 0;

	internal const int RTLD_DEFAULT = 258;

	private static readonly char[] utsNameSeparators = new char[1];

	[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
	private static extern int uname(out utsname_interop name);

	[DllImport("__Internal", BestFitMapping = false, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, SetLastError = true, ThrowOnUnmappableChar = true)]
	internal static extern IntPtr dlopen(string fileName, int mode);

	[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
	internal static extern int dlclose(IntPtr module);

	internal static bool GetOsVersionInfo(ref utsname utsName)
	{
		try
		{
			if (uname(out var name) < 0)
			{
				return false;
			}
			if (name.buffer == null)
			{
				return false;
			}
			string @string = Encoding.UTF8.GetString(name.buffer);
			if (@string == null || utsNameSeparators == null)
			{
				return false;
			}
			@string = @string.Trim(utsNameSeparators);
			string[] array = @string.Split(utsNameSeparators, StringSplitOptions.RemoveEmptyEntries);
			if (array == null)
			{
				return false;
			}
			utsname utsname = new utsname();
			if (array.Length >= 1)
			{
				utsname.sysname = array[0];
			}
			if (array.Length >= 2)
			{
				utsname.nodename = array[1];
			}
			if (array.Length >= 3)
			{
				utsname.release = array[2];
			}
			if (array.Length >= 4)
			{
				utsname.version = array[3];
			}
			if (array.Length >= 5)
			{
				utsname.machine = array[4];
			}
			utsName = utsname;
			return true;
		}
		catch
		{
		}
		return false;
	}
}
