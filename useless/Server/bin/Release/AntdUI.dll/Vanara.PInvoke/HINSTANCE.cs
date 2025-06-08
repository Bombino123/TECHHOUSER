using System;
using System.Diagnostics;

namespace Vanara.PInvoke;

[DebuggerDisplay("{handle}")]
public struct HINSTANCE : IKernelHandle, IHandle
{
	private readonly IntPtr handle;

	public static HINSTANCE NULL => new HINSTANCE(IntPtr.Zero);

	public bool IsNull => handle == IntPtr.Zero;

	public HINSTANCE(IntPtr preexistingHandle)
	{
		handle = preexistingHandle;
	}

	public static explicit operator IntPtr(HINSTANCE h)
	{
		return h.handle;
	}

	public static implicit operator HINSTANCE(IntPtr h)
	{
		return new HINSTANCE(h);
	}

	public static bool operator !=(HINSTANCE h1, HINSTANCE h2)
	{
		return !(h1 == h2);
	}

	public static bool operator ==(HINSTANCE h1, HINSTANCE h2)
	{
		return h1.Equals(h2);
	}

	public override bool Equals(object obj)
	{
		if (obj is HINSTANCE hINSTANCE)
		{
			return handle == hINSTANCE.handle;
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
