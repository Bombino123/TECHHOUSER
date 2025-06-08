using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
public sealed class PdbIteratorMethodCustomDebugInfo : PdbCustomDebugInfo
{
	public override PdbCustomDebugInfoKind Kind => PdbCustomDebugInfoKind.IteratorMethod;

	public override Guid Guid => Guid.Empty;

	public MethodDef KickoffMethod { get; set; }

	public PdbIteratorMethodCustomDebugInfo()
	{
	}

	public PdbIteratorMethodCustomDebugInfo(MethodDef kickoffMethod)
	{
		KickoffMethod = kickoffMethod;
	}
}
