using dnlib.DotNet.Pdb.Symbols;

namespace dnlib.DotNet.Pdb.Portable;

internal sealed class SymbolVariableImpl : SymbolVariable
{
	private readonly string name;

	private readonly PdbLocalAttributes attributes;

	private readonly int index;

	private readonly PdbCustomDebugInfo[] customDebugInfos;

	public override string Name => name;

	public override PdbLocalAttributes Attributes => attributes;

	public override int Index => index;

	public override PdbCustomDebugInfo[] CustomDebugInfos => customDebugInfos;

	public SymbolVariableImpl(string name, PdbLocalAttributes attributes, int index, PdbCustomDebugInfo[] customDebugInfos)
	{
		this.name = name;
		this.attributes = attributes;
		this.index = index;
		this.customDebugInfos = customDebugInfos;
	}
}
