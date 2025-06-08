using System;
using System.Collections.Generic;

namespace dnlib.DotNet.Pdb;

public sealed class PdbEditAndContinueStateMachineStateMapDebugInfo : PdbCustomDebugInfo
{
	public override PdbCustomDebugInfoKind Kind => PdbCustomDebugInfoKind.EditAndContinueStateMachineStateMap;

	public override Guid Guid => CustomDebugInfoGuids.EncStateMachineStateMap;

	public List<StateMachineStateInfo> StateMachineStates { get; }

	public PdbEditAndContinueStateMachineStateMapDebugInfo()
	{
		StateMachineStates = new List<StateMachineStateInfo>();
	}
}
