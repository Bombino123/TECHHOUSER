using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
public class PdbTypeDefinitionDocumentsDebugInfo : PdbCustomDebugInfo
{
	protected IList<PdbDocument> documents;

	public override PdbCustomDebugInfoKind Kind => PdbCustomDebugInfoKind.TypeDefinitionDocuments;

	public override Guid Guid => CustomDebugInfoGuids.TypeDefinitionDocuments;

	public IList<PdbDocument> Documents
	{
		get
		{
			if (documents == null)
			{
				InitializeDocuments();
			}
			return documents;
		}
	}

	protected virtual void InitializeDocuments()
	{
		Interlocked.CompareExchange(ref documents, new List<PdbDocument>(), null);
	}
}
