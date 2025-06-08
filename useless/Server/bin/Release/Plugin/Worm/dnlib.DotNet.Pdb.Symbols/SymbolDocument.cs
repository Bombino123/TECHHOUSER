using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Symbols;

[ComVisible(true)]
public abstract class SymbolDocument
{
	public abstract string URL { get; }

	public abstract Guid Language { get; }

	public abstract Guid LanguageVendor { get; }

	public abstract Guid DocumentType { get; }

	public abstract Guid CheckSumAlgorithmId { get; }

	public abstract byte[] CheckSum { get; }

	public abstract PdbCustomDebugInfo[] CustomDebugInfos { get; }

	public abstract MDToken? MDToken { get; }
}
