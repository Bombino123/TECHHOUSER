using System;
using System.Runtime.InteropServices;

namespace Vestris.ResourceLib;

internal sealed class Aligned : IDisposable
{
	private IntPtr _ptr;

	private bool allocated;

	private bool Disposed => _ptr == IntPtr.Zero;

	public IntPtr Ptr
	{
		get
		{
			if (Disposed)
			{
				throw new ObjectDisposedException("Aligned");
			}
			return _ptr;
		}
	}

	public Aligned(IntPtr lp, int size)
	{
		if (lp == IntPtr.Zero)
		{
			throw new ArgumentException("Cannot align a null pointer.", "lp");
		}
		if (lp.ToInt64() % 8 == 0L)
		{
			_ptr = lp;
			allocated = false;
		}
		else
		{
			_ptr = Marshal.AllocHGlobal(size);
			allocated = true;
			Kernel32.MoveMemory(_ptr, lp, (uint)size);
		}
	}

	public void Dispose()
	{
		if (allocated && !Disposed)
		{
			Marshal.FreeHGlobal(_ptr);
			_ptr = IntPtr.Zero;
		}
	}
}
