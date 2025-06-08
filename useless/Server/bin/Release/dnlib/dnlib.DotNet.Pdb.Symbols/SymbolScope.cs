using System.Collections.Generic;

namespace dnlib.DotNet.Pdb.Symbols;

public abstract class SymbolScope
{
	public abstract SymbolMethod Method { get; }

	public abstract SymbolScope Parent { get; }

	public abstract int StartOffset { get; }

	public abstract int EndOffset { get; }

	public abstract IList<SymbolScope> Children { get; }

	public abstract IList<SymbolVariable> Locals { get; }

	public abstract IList<SymbolNamespace> Namespaces { get; }

	public abstract IList<PdbCustomDebugInfo> CustomDebugInfos { get; }

	public abstract PdbImportScope ImportScope { get; }

	public abstract IList<PdbConstant> GetConstants(ModuleDef module, GenericParamContext gpContext);
}
