using System;
using System.Collections.Generic;

namespace dnlib.DotNet.Pdb;

public sealed class PdbCompilationMetadataReferencesCustomDebugInfo : PdbCustomDebugInfo
{
	public override PdbCustomDebugInfoKind Kind => PdbCustomDebugInfoKind.CompilationMetadataReferences;

	public override Guid Guid => CustomDebugInfoGuids.CompilationMetadataReferences;

	public List<PdbCompilationMetadataReference> References { get; }

	public PdbCompilationMetadataReferencesCustomDebugInfo()
	{
		References = new List<PdbCompilationMetadataReference>();
	}
}
