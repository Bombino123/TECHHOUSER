using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

public struct AuthenticatedQueryAcessibilityOutput
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public AuthenticatedQueryOutput.__Native Output;

		public BusType BusType;

		public RawBool AccessibleInContiguousBlocks;

		public RawBool AccessibleInNonContiguousBlocks;
	}

	public AuthenticatedQueryOutput Output;

	public BusType BusType;

	public RawBool AccessibleInContiguousBlocks;

	public RawBool AccessibleInNonContiguousBlocks;

	internal void __MarshalFree(ref __Native @ref)
	{
		Output.__MarshalFree(ref @ref.Output);
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		Output.__MarshalFrom(ref @ref.Output);
		BusType = @ref.BusType;
		AccessibleInContiguousBlocks = @ref.AccessibleInContiguousBlocks;
		AccessibleInNonContiguousBlocks = @ref.AccessibleInNonContiguousBlocks;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		Output.__MarshalTo(ref @ref.Output);
		@ref.BusType = BusType;
		@ref.AccessibleInContiguousBlocks = AccessibleInContiguousBlocks;
		@ref.AccessibleInNonContiguousBlocks = AccessibleInNonContiguousBlocks;
	}
}
