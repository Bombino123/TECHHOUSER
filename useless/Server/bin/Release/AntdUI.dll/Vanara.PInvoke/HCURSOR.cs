using System;
using System.Diagnostics;

namespace Vanara.PInvoke;

[DebuggerDisplay("{handle}")]
public struct HCURSOR : IGraphicsObjectHandle, IUserHandle, IHandle
{
	private readonly IntPtr handle;

	public static HCURSOR NULL => new HCURSOR(IntPtr.Zero);

	public bool IsNull => handle == IntPtr.Zero;

	public HCURSOR(IntPtr preexistingHandle)
	{
		handle = preexistingHandle;
	}

	public static explicit operator IntPtr(HCURSOR h)
	{
		return h.handle;
	}

	public static implicit operator HCURSOR(IntPtr h)
	{
		return new HCURSOR(h);
	}

	public static bool operator !=(HCURSOR h1, HCURSOR h2)
	{
		return !(h1 == h2);
	}

	public static bool operator ==(HCURSOR h1, HCURSOR h2)
	{
		return h1.Equals(h2);
	}

	public override bool Equals(object obj)
	{
		if (obj is HCURSOR hCURSOR)
		{
			return handle == hCURSOR.handle;
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
