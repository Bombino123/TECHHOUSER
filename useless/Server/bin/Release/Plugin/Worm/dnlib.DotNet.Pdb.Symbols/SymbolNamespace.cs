using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Symbols;

[ComVisible(true)]
public abstract class SymbolNamespace
{
	public abstract string Name { get; }
}
