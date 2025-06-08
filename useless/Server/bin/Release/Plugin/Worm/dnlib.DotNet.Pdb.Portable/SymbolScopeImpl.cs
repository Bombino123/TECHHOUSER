using System;
using System.Collections.Generic;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb.Symbols;

namespace dnlib.DotNet.Pdb.Portable;

internal sealed class SymbolScopeImpl : SymbolScope
{
	private readonly PortablePdbReader owner;

	internal SymbolMethod method;

	private readonly SymbolScopeImpl parent;

	private readonly int startOffset;

	private readonly int endOffset;

	internal readonly List<SymbolScope> childrenList;

	internal readonly List<SymbolVariable> localsList;

	internal PdbImportScope importScope;

	private readonly PdbCustomDebugInfo[] customDebugInfos;

	private Metadata constantsMetadata;

	private RidList constantRidList;

	public override SymbolMethod Method
	{
		get
		{
			if (method != null)
			{
				return method;
			}
			SymbolScopeImpl symbolScopeImpl = parent;
			if (symbolScopeImpl == null)
			{
				return method;
			}
			while (symbolScopeImpl.parent != null)
			{
				symbolScopeImpl = symbolScopeImpl.parent;
			}
			return method = symbolScopeImpl.method;
		}
	}

	public override SymbolScope Parent => parent;

	public override int StartOffset => startOffset;

	public override int EndOffset => endOffset;

	public override IList<SymbolScope> Children => childrenList;

	public override IList<SymbolVariable> Locals => localsList;

	public override IList<SymbolNamespace> Namespaces => Array2.Empty<SymbolNamespace>();

	public override IList<PdbCustomDebugInfo> CustomDebugInfos => customDebugInfos;

	public override PdbImportScope ImportScope => importScope;

	public SymbolScopeImpl(PortablePdbReader owner, SymbolScopeImpl parent, int startOffset, int endOffset, PdbCustomDebugInfo[] customDebugInfos)
	{
		this.owner = owner;
		method = null;
		this.parent = parent;
		this.startOffset = startOffset;
		this.endOffset = endOffset;
		childrenList = new List<SymbolScope>();
		localsList = new List<SymbolVariable>();
		this.customDebugInfos = customDebugInfos;
	}

	internal void SetConstants(Metadata metadata, RidList rids)
	{
		constantsMetadata = metadata;
		constantRidList = rids;
	}

	public override IList<PdbConstant> GetConstants(ModuleDef module, GenericParamContext gpContext)
	{
		if (constantRidList.Count == 0)
		{
			return Array2.Empty<PdbConstant>();
		}
		PdbConstant[] array = new PdbConstant[constantRidList.Count];
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			uint rid = constantRidList[i];
			constantsMetadata.TablesStream.TryReadLocalConstantRow(rid, out var row);
			UTF8String uTF8String = constantsMetadata.StringsStream.Read(row.Name);
			if (constantsMetadata.BlobStream.TryCreateReader(row.Signature, out var reader) && new LocalConstantSigBlobReader(module, ref reader, gpContext).Read(out var type, out var value))
			{
				PdbConstant pdbConstant = new PdbConstant(uTF8String, type, value);
				int token = new MDToken(Table.LocalConstant, rid).ToInt32();
				owner.GetCustomDebugInfos(token, gpContext, pdbConstant.CustomDebugInfos);
				array[num++] = pdbConstant;
			}
		}
		if (array.Length != num)
		{
			Array.Resize(ref array, num);
		}
		return array;
	}
}
