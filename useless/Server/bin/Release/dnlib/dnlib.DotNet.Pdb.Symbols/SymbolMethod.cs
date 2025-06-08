using System.Collections.Generic;
using dnlib.DotNet.Emit;

namespace dnlib.DotNet.Pdb.Symbols;

public abstract class SymbolMethod
{
	public abstract int Token { get; }

	public abstract SymbolScope RootScope { get; }

	public abstract IList<SymbolSequencePoint> SequencePoints { get; }

	public abstract void GetCustomDebugInfos(MethodDef method, CilBody body, IList<PdbCustomDebugInfo> result);
}
