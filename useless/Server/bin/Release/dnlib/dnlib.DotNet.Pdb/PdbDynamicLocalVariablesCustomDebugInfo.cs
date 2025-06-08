using System;

namespace dnlib.DotNet.Pdb;

public sealed class PdbDynamicLocalVariablesCustomDebugInfo : PdbCustomDebugInfo
{
	public override PdbCustomDebugInfoKind Kind => PdbCustomDebugInfoKind.DynamicLocalVariables;

	public override Guid Guid => CustomDebugInfoGuids.DynamicLocalVariables;

	public bool[] Flags { get; set; }

	public PdbDynamicLocalVariablesCustomDebugInfo()
	{
	}

	public PdbDynamicLocalVariablesCustomDebugInfo(bool[] flags)
	{
		Flags = flags;
	}
}
