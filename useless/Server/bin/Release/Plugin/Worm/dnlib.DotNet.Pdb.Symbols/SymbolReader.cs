using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Symbols;

[ComVisible(true)]
public abstract class SymbolReader : IDisposable
{
	public abstract PdbFileKind PdbFileKind { get; }

	public abstract int UserEntryPoint { get; }

	public abstract IList<SymbolDocument> Documents { get; }

	public abstract void Initialize(ModuleDef module);

	public abstract SymbolMethod GetMethod(MethodDef method, int version);

	public abstract void GetCustomDebugInfos(int token, GenericParamContext gpContext, IList<PdbCustomDebugInfo> result);

	public virtual void Dispose()
	{
	}
}
