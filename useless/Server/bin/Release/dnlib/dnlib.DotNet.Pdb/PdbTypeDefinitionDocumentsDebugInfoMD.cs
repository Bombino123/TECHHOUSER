using System.Collections.Generic;
using System.Threading;

namespace dnlib.DotNet.Pdb;

internal sealed class PdbTypeDefinitionDocumentsDebugInfoMD : PdbTypeDefinitionDocumentsDebugInfo
{
	private readonly ModuleDef readerModule;

	private readonly IList<MDToken> documentTokens;

	protected override void InitializeDocuments()
	{
		List<PdbDocument> list = new List<PdbDocument>(documentTokens.Count);
		if (readerModule.PdbState != null)
		{
			for (int i = 0; i < documentTokens.Count; i++)
			{
				if (readerModule.PdbState.tokenToDocument.TryGetValue(documentTokens[i], out var value))
				{
					list.Add(value);
				}
			}
		}
		Interlocked.CompareExchange(ref documents, list, null);
	}

	public PdbTypeDefinitionDocumentsDebugInfoMD(ModuleDef readerModule, IList<MDToken> documentTokens)
	{
		this.readerModule = readerModule;
		this.documentTokens = documentTokens;
	}
}
