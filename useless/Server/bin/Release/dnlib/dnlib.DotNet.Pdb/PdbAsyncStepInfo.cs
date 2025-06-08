using dnlib.DotNet.Emit;

namespace dnlib.DotNet.Pdb;

public struct PdbAsyncStepInfo
{
	public Instruction YieldInstruction;

	public MethodDef BreakpointMethod;

	public Instruction BreakpointInstruction;

	public PdbAsyncStepInfo(Instruction yieldInstruction, MethodDef breakpointMethod, Instruction breakpointInstruction)
	{
		YieldInstruction = yieldInstruction;
		BreakpointMethod = breakpointMethod;
		BreakpointInstruction = breakpointInstruction;
	}
}
