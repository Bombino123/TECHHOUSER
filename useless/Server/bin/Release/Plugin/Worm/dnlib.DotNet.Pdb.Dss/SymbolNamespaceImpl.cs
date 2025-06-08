using dnlib.DotNet.Pdb.Symbols;

namespace dnlib.DotNet.Pdb.Dss;

internal sealed class SymbolNamespaceImpl : SymbolNamespace
{
	private readonly ISymUnmanagedNamespace ns;

	public override string Name
	{
		get
		{
			ns.GetName(0u, out var pcchName, null);
			char[] array = new char[pcchName];
			ns.GetName((uint)array.Length, out pcchName, array);
			if (array.Length == 0)
			{
				return string.Empty;
			}
			return new string(array, 0, array.Length - 1);
		}
	}

	public SymbolNamespaceImpl(ISymUnmanagedNamespace @namespace)
	{
		ns = @namespace;
	}
}
