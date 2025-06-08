using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
public abstract class PdbCustomDebugInfo
{
	public abstract PdbCustomDebugInfoKind Kind { get; }

	public abstract Guid Guid { get; }
}
