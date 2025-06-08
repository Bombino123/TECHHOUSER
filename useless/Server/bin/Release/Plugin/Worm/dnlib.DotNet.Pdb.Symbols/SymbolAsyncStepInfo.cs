using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Symbols;

[ComVisible(true)]
public struct SymbolAsyncStepInfo
{
	public uint YieldOffset;

	public uint BreakpointOffset;

	public uint BreakpointMethod;

	public SymbolAsyncStepInfo(uint yieldOffset, uint breakpointOffset, uint breakpointMethod)
	{
		YieldOffset = yieldOffset;
		BreakpointOffset = breakpointOffset;
		BreakpointMethod = breakpointMethod;
	}
}
