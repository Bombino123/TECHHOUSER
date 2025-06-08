using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

public struct AuthenticatedQueryUnrestrictedProtectedSharedResourceCountOutput
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public AuthenticatedQueryOutput.__Native Output;

		public int UnrestrictedProtectedSharedResourceCount;
	}

	public AuthenticatedQueryOutput Output;

	public int UnrestrictedProtectedSharedResourceCount;

	internal void __MarshalFree(ref __Native @ref)
	{
		Output.__MarshalFree(ref @ref.Output);
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		Output.__MarshalFrom(ref @ref.Output);
		UnrestrictedProtectedSharedResourceCount = @ref.UnrestrictedProtectedSharedResourceCount;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		Output.__MarshalTo(ref @ref.Output);
		@ref.UnrestrictedProtectedSharedResourceCount = UnrestrictedProtectedSharedResourceCount;
	}
}
