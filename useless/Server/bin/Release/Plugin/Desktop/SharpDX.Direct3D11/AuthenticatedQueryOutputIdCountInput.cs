using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct AuthenticatedQueryOutputIdCountInput
{
	public AuthenticatedQueryInput Input;

	public IntPtr DeviceHandle;

	public IntPtr CryptoSessionHandle;
}
