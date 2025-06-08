using System;

namespace dnlib.DotNet.Pdb;

public abstract class PdbCustomDebugInfo
{
	public abstract PdbCustomDebugInfoKind Kind { get; }

	public abstract Guid Guid { get; }
}
