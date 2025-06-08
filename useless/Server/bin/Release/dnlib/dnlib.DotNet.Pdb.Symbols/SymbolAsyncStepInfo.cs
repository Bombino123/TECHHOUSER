namespace dnlib.DotNet.Pdb.Symbols;

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
