using System;

namespace Vanara.PInvoke;

public static class Macros
{
	[PInvokeData("WinBase.h", MSDNShortId = "ms648028")]
	public static bool IS_INTRESOURCE(IntPtr ptr)
	{
		return ptr.ToInt64() >>> 16 == 0;
	}

	public static byte LOBYTE(ushort wValue)
	{
		return (byte)(wValue & 0xFFu);
	}

	public static uint LowPart(this long lValue)
	{
		return (uint)(lValue & 0xFFFFFFFFu);
	}

	public static ushort LOWORD(uint dwValue)
	{
		return (ushort)(dwValue & 0xFFFFu);
	}

	public static ushort LOWORD(IntPtr dwValue)
	{
		return (ushort)(long)dwValue;
	}

	public static ushort LOWORD(UIntPtr dwValue)
	{
		return (ushort)(ulong)dwValue;
	}

	[PInvokeData("WinUser.h", MSDNShortId = "ms648029")]
	public static ResourceId MAKEINTRESOURCE(int id)
	{
		return id;
	}

	public static int MAKELONG(int wLow, int wHigh)
	{
		return (wHigh << 16) | (wLow & 0xFFFF);
	}

	public static long MAKELONG64(long dwLow, long dwHigh)
	{
		return (dwHigh << 32) | (dwLow & 0xFFFFFFFFu);
	}

	public static IntPtr MAKELPARAM(int wLow, int wHigh)
	{
		return new IntPtr(MAKELONG(wLow, wHigh));
	}
}
