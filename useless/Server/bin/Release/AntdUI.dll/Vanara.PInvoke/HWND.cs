using System;
using System.Diagnostics;

namespace Vanara.PInvoke;

[DebuggerDisplay("{handle}")]
public struct HWND : IUserHandle, IHandle
{
	private readonly IntPtr handle;

	public static HWND HWND_BOTTOM = new IntPtr(1);

	public static HWND HWND_MESSAGE = new IntPtr(-3);

	public static HWND HWND_NOTOPMOST = new IntPtr(-2);

	public static HWND HWND_TOP = new IntPtr(0);

	public static HWND HWND_TOPMOST = new IntPtr(-1);

	public static HWND NULL => new HWND(IntPtr.Zero);

	public bool IsNull => handle == IntPtr.Zero;

	public HWND(IntPtr preexistingHandle)
	{
		handle = preexistingHandle;
	}

	public static explicit operator IntPtr(HWND h)
	{
		return h.handle;
	}

	public static implicit operator HWND(IntPtr h)
	{
		return new HWND(h);
	}

	public static bool operator !=(HWND h1, HWND h2)
	{
		return !(h1 == h2);
	}

	public static bool operator ==(HWND h1, HWND h2)
	{
		return h1.Equals(h2);
	}

	public override bool Equals(object obj)
	{
		if (obj is HWND hWND)
		{
			return handle == hWND.handle;
		}
		return false;
	}

	public override int GetHashCode()
	{
		IntPtr intPtr = handle;
		return intPtr.GetHashCode();
	}

	public IntPtr DangerousGetHandle()
	{
		return handle;
	}
}
