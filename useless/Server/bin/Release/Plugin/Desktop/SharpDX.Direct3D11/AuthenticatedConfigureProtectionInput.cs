using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

public struct AuthenticatedConfigureProtectionInput
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public AuthenticatedConfigureInput.__Native Parameters;

		public AuthenticatedProtectionFlags Protections;
	}

	public AuthenticatedConfigureInput Parameters;

	public AuthenticatedProtectionFlags Protections;

	internal void __MarshalFree(ref __Native @ref)
	{
		Parameters.__MarshalFree(ref @ref.Parameters);
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		Parameters.__MarshalFrom(ref @ref.Parameters);
		Protections = @ref.Protections;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		Parameters.__MarshalTo(ref @ref.Parameters);
		@ref.Protections = Protections;
	}
}
