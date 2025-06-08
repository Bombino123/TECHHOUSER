using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("5c1e0d8a-7c23-48f9-8c59-a92958ceff11")]
public class DeviceContextState : DeviceChild
{
	public DeviceContextState(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DeviceContextState(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DeviceContextState(nativePtr);
		}
		return null;
	}
}
