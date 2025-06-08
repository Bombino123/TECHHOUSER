using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
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
