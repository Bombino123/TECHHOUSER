using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct AuthenticatedQueryInput
{
	public Guid QueryType;

	public IntPtr HChannel;

	public int SequenceNumber;
}
