using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("2411e7e1-12ac-4ccf-bd14-9798e8534dc0")]
public class Adapter : DXGIObject
{
	public Output[] Outputs
	{
		get
		{
			List<Output> list = new List<Output>();
			Output outputOut;
			while (!(GetOutput(list.Count, out outputOut) == ResultCode.NotFound) && outputOut != null)
			{
				list.Add(outputOut);
			}
			return list.ToArray();
		}
	}

	public AdapterDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public bool IsInterfaceSupported(Type type)
	{
		long userModeVersion;
		return IsInterfaceSupported(type, out userModeVersion);
	}

	public bool IsInterfaceSupported<T>() where T : ComObject
	{
		long userModeVersion;
		return IsInterfaceSupported(typeof(T), out userModeVersion);
	}

	public bool IsInterfaceSupported<T>(out long userModeVersion) where T : ComObject
	{
		return IsInterfaceSupported(typeof(T), out userModeVersion);
	}

	public bool IsInterfaceSupported(Type type, out long userModeVersion)
	{
		return CheckInterfaceSupport(Utilities.GetGuidFromType(type), out userModeVersion).Success;
	}

	public Output GetOutput(int outputIndex)
	{
		GetOutput(outputIndex, out var outputOut).CheckError();
		return outputOut;
	}

	public int GetOutputCount()
	{
		int i;
		Output outputOut;
		for (i = 0; !(GetOutput(i, out outputOut) == ResultCode.NotFound); i++)
		{
			if (outputOut == null)
			{
				break;
			}
			outputOut.Dispose();
		}
		return i;
	}

	public Adapter(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Adapter(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Adapter(nativePtr);
		}
		return null;
	}

	internal unsafe Result GetOutput(int output, out Output outputOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, output, &zero);
		if (zero != IntPtr.Zero)
		{
			outputOut = new Output(zero);
			return result;
		}
		outputOut = null;
		return result;
	}

	internal unsafe void GetDescription(out AdapterDescription descRef)
	{
		AdapterDescription.__Native @ref = default(AdapterDescription.__Native);
		descRef = default(AdapterDescription);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, &@ref);
		descRef.__MarshalFrom(ref @ref);
		result.CheckError();
	}

	internal unsafe Result CheckInterfaceSupport(Guid interfaceName, out long uMDVersionRef)
	{
		Result result;
		fixed (long* ptr = &uMDVersionRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, &interfaceName, ptr2);
		}
		return result;
	}
}
