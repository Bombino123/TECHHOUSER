using System.Runtime.InteropServices;
using dnlib.DotNet.Emit;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
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
