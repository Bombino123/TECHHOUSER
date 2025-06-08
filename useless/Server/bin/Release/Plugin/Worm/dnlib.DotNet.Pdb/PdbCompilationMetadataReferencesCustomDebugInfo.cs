using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
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
