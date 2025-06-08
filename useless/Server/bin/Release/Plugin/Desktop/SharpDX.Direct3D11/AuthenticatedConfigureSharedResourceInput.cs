using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

public struct AuthenticatedConfigureSharedResourceInput
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public AuthenticatedConfigureInput.__Native Parameters;

		public AuthenticatedProcessIdentifierType ProcessType;

		public IntPtr ProcessHandle;

		public RawBool AllowAccess;
	}

	public AuthenticatedConfigureInput Parameters;

	public AuthenticatedProcessIdentifierType ProcessType;

	public IntPtr ProcessHandle;

	public RawBool AllowAccess;

	internal void __MarshalFree(ref __Native @ref)
	{
		Parameters.__MarshalFree(ref @ref.Parameters);
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		Parameters.__MarshalFrom(ref @ref.Parameters);
		ProcessType = @ref.ProcessType;
		ProcessHandle = @ref.ProcessHandle;
		AllowAccess = @ref.AllowAccess;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		Parameters.__MarshalTo(ref @ref.Parameters);
		@ref.ProcessType = ProcessType;
		@ref.ProcessHandle = ProcessHandle;
		@ref.AllowAccess = AllowAccess;
	}
}
