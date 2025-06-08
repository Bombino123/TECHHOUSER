namespace System.Data.SQLite;

internal static class NativeLibraryHelper
{
	private delegate IntPtr LoadLibraryCallback(string fileName);

	private delegate string GetMachineCallback();

	private static IntPtr LoadLibraryWin32(string fileName)
	{
		return UnsafeNativeMethodsWin32.LoadLibrary(fileName);
	}

	private static string GetMachineWin32()
	{
		try
		{
			UnsafeNativeMethodsWin32.GetSystemInfo(out var systemInfo);
			return systemInfo.wProcessorArchitecture.ToString();
		}
		catch
		{
		}
		return null;
	}

	private static IntPtr LoadLibraryPosix(string fileName)
	{
		return UnsafeNativeMethodsPosix.dlopen(fileName, 258);
	}

	private static string GetMachinePosix()
	{
		try
		{
			UnsafeNativeMethodsPosix.utsname utsName = null;
			if (UnsafeNativeMethodsPosix.GetOsVersionInfo(ref utsName) && utsName != null)
			{
				return utsName.machine;
			}
		}
		catch
		{
		}
		return null;
	}

	public static IntPtr LoadLibrary(string fileName)
	{
		LoadLibraryCallback loadLibraryCallback = LoadLibraryWin32;
		if (!HelperMethods.IsWindows())
		{
			loadLibraryCallback = LoadLibraryPosix;
		}
		return loadLibraryCallback(fileName);
	}

	public static string GetMachine()
	{
		GetMachineCallback getMachineCallback = GetMachineWin32;
		if (!HelperMethods.IsWindows())
		{
			getMachineCallback = GetMachinePosix;
		}
		return getMachineCallback();
	}
}
