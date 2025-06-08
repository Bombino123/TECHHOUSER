using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
public sealed class PdbDynamicLocalsCustomDebugInfo : PdbCustomDebugInfo
{
	private readonly IList<PdbDynamicLocal> locals;

	public override PdbCustomDebugInfoKind Kind => PdbCustomDebugInfoKind.DynamicLocals;

	public override Guid Guid => Guid.Empty;

	public IList<PdbDynamicLocal> Locals => locals;

	public PdbDynamicLocalsCustomDebugInfo()
	{
		locals = new List<PdbDynamicLocal>();
	}

	public PdbDynamicLocalsCustomDebugInfo(int capacity)
	{
		locals = new List<PdbDynamicLocal>(capacity);
	}
}
