using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
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
