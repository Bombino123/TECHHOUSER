using dnlib.DotNet.Pdb.Symbols;

namespace dnlib.DotNet.Pdb.Managed;

internal sealed class DbiNamespace : SymbolNamespace
{
	private readonly string name;

	public override string Name => name;

	public DbiNamespace(string ns)
	{
		name = ns;
	}
}
