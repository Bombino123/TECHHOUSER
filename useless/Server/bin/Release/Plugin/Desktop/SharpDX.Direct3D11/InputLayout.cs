using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("e4819ddc-4cf0-4025-bd26-5de82a3e07b7")]
public class InputLayout : DeviceChild
{
	public unsafe InputLayout(Device device, byte[] shaderBytecode, InputElement[] elements)
		: base(IntPtr.Zero)
	{
		fixed (byte* ptr = shaderBytecode)
		{
			void* ptr2 = ptr;
			device.CreateInputLayout(elements, elements.Length, (IntPtr)ptr2, shaderBytecode.Length, this);
		}
	}

	public InputLayout(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator InputLayout(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new InputLayout(nativePtr);
		}
		return null;
	}
}
