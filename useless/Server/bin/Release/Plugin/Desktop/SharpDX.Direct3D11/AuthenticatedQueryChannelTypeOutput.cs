using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

public struct AuthenticatedQueryChannelTypeOutput
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public AuthenticatedQueryOutput.__Native Output;

		public AuthenticatedChannelType ChannelType;
	}

	public AuthenticatedQueryOutput Output;

	public AuthenticatedChannelType ChannelType;

	internal void __MarshalFree(ref __Native @ref)
	{
		Output.__MarshalFree(ref @ref.Output);
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		Output.__MarshalFrom(ref @ref.Output);
		ChannelType = @ref.ChannelType;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		Output.__MarshalTo(ref @ref.Output);
		@ref.ChannelType = ChannelType;
	}
}
