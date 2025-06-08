using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("a24bc4d1-769e-43f7-8013-98ff566c18e2")]
public class CommandList : DeviceChild
{
	public CommandList(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator CommandList(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new CommandList(nativePtr);
		}
		return null;
	}
}
