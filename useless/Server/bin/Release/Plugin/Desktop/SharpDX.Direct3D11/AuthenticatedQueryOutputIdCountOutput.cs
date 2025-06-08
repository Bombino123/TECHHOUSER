using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

public struct AuthenticatedQueryOutputIdCountOutput
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public AuthenticatedQueryOutput.__Native Output;

		public IntPtr DeviceHandle;

		public IntPtr CryptoSessionHandle;

		public int OutputIDCount;
	}

	public AuthenticatedQueryOutput Output;

	public IntPtr DeviceHandle;

	public IntPtr CryptoSessionHandle;

	public int OutputIDCount;

	internal void __MarshalFree(ref __Native @ref)
	{
		Output.__MarshalFree(ref @ref.Output);
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		Output.__MarshalFrom(ref @ref.Output);
		DeviceHandle = @ref.DeviceHandle;
		CryptoSessionHandle = @ref.CryptoSessionHandle;
		OutputIDCount = @ref.OutputIDCount;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		Output.__MarshalTo(ref @ref.Output);
		@ref.DeviceHandle = DeviceHandle;
		@ref.CryptoSessionHandle = CryptoSessionHandle;
		@ref.OutputIDCount = OutputIDCount;
	}
}
