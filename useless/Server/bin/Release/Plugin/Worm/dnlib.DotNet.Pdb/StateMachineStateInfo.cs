using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
public struct StateMachineStateInfo
{
	public readonly int SyntaxOffset;

	public readonly StateMachineState State;

	public StateMachineStateInfo(int syntaxOffset, StateMachineState state)
	{
		SyntaxOffset = syntaxOffset;
		State = state;
	}
}
