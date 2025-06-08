using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

public struct AuthenticatedQueryOutputIdOutput
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public AuthenticatedQueryOutput.__Native Output;

		public IntPtr DeviceHandle;

		public IntPtr CryptoSessionHandle;

		public int OutputIDIndex;

		public long OutputID;
	}

	public AuthenticatedQueryOutput Output;

	public IntPtr DeviceHandle;

	public IntPtr CryptoSessionHandle;

	public int OutputIDIndex;

	public long OutputID;

	internal void __MarshalFree(ref __Native @ref)
	{
		Output.__MarshalFree(ref @ref.Output);
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		Output.__MarshalFrom(ref @ref.Output);
		DeviceHandle = @ref.DeviceHandle;
		CryptoSessionHandle = @ref.CryptoSessionHandle;
		OutputIDIndex = @ref.OutputIDIndex;
		OutputID = @ref.OutputID;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		Output.__MarshalTo(ref @ref.Output);
		@ref.DeviceHandle = DeviceHandle;
		@ref.CryptoSessionHandle = CryptoSessionHandle;
		@ref.OutputIDIndex = OutputIDIndex;
		@ref.OutputID = OutputID;
	}
}
