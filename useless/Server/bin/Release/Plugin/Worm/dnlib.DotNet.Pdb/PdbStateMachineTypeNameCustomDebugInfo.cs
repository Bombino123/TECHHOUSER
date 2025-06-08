using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
public sealed class PdbStateMachineTypeNameCustomDebugInfo : PdbCustomDebugInfo
{
	public override PdbCustomDebugInfoKind Kind => PdbCustomDebugInfoKind.StateMachineTypeName;

	public override Guid Guid => Guid.Empty;

	public TypeDef Type { get; set; }

	public PdbStateMachineTypeNameCustomDebugInfo()
	{
	}

	public PdbStateMachineTypeNameCustomDebugInfo(TypeDef type)
	{
		Type = type;
	}
}
