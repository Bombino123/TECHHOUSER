using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Vanara.PInvoke;

[DebuggerDisplay("{handle}")]
public abstract class SafeHANDLE : SafeHandleZeroOrMinusOneIsInvalid, IEquatable<SafeHANDLE>, IHandle
{
	public bool IsNull => handle == IntPtr.Zero;

	public SafeHANDLE()
		: base(ownsHandle: true)
	{
	}

	protected SafeHANDLE(IntPtr preexistingHandle, bool ownsHandle = true)
		: base(ownsHandle)
	{
		SetHandle(preexistingHandle);
	}

	public static bool operator !=(SafeHANDLE h1, IHandle h2)
	{
		return !(h1 == h2);
	}

	public static bool operator ==(SafeHANDLE h1, IHandle h2)
	{
		return h1?.Equals(h2) ?? (h2 == null);
	}

	public static bool operator !=(SafeHANDLE h1, IntPtr h2)
	{
		return !(h1 == h2);
	}

	public static bool operator ==(SafeHANDLE h1, IntPtr h2)
	{
		return h1?.Equals(h2) ?? false;
	}

	public bool Equals(SafeHANDLE? other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		if (handle == other.handle)
		{
			return base.IsClosed == other.IsClosed;
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		if (!(obj is IHandle handle))
		{
			if (!(obj is SafeHandle safeHandle))
			{
				if (obj is IntPtr intPtr)
				{
					return base.handle.Equals((object?)(nint)intPtr);
				}
				return base.Equals(obj);
			}
			return base.handle.Equals((object?)(nint)safeHandle.DangerousGetHandle());
		}
		return base.handle.Equals((object?)(nint)handle.DangerousGetHandle());
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public IntPtr ReleaseOwnership()
	{
		IntPtr result = handle;
		SetHandleAsInvalid();
		return result;
	}

	protected abstract bool InternalReleaseHandle();

	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
	protected override bool ReleaseHandle()
	{
		if (IsInvalid)
		{
			return true;
		}
		if (!InternalReleaseHandle())
		{
			return false;
		}
		handle = IntPtr.Zero;
		return true;
	}

	IntPtr IHandle.DangerousGetHandle()
	{
		return DangerousGetHandle();
	}
}
