using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct KeyExchangeHwProtectionData
{
	public int HWProtectionFunctionID;

	public IntPtr PInputData;

	public IntPtr POutputData;

	public Result Status;
}
