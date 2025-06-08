using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

public struct AuthenticatedConfigureInitializeInput
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public AuthenticatedConfigureInput.__Native Parameters;

		public int StartSequenceQuery;

		public int StartSequenceConfigure;
	}

	public AuthenticatedConfigureInput Parameters;

	public int StartSequenceQuery;

	public int StartSequenceConfigure;

	internal void __MarshalFree(ref __Native @ref)
	{
		Parameters.__MarshalFree(ref @ref.Parameters);
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		Parameters.__MarshalFrom(ref @ref.Parameters);
		StartSequenceQuery = @ref.StartSequenceQuery;
		StartSequenceConfigure = @ref.StartSequenceConfigure;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		Parameters.__MarshalTo(ref @ref.Parameters);
		@ref.StartSequenceQuery = StartSequenceQuery;
		@ref.StartSequenceConfigure = StartSequenceConfigure;
	}
}
