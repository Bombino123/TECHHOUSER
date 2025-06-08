using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct QueryDescription1
{
	public QueryType Query;

	public int MiscFlags;

	public ContextType ContextType;
}
