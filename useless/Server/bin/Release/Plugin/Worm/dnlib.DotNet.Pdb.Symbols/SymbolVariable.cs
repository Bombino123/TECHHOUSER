using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Symbols;

[ComVisible(true)]
public abstract class SymbolVariable
{
	public abstract string Name { get; }

	public abstract PdbLocalAttributes Attributes { get; }

	public abstract int Index { get; }

	public abstract PdbCustomDebugInfo[] CustomDebugInfos { get; }
}
