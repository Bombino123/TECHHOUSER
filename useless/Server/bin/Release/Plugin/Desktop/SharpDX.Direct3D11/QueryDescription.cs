using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct QueryDescription
{
	public QueryType Type;

	public QueryFlags Flags;
}
