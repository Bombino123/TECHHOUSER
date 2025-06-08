using System;
using System.Collections.Generic;

namespace dnlib.DotNet.Pdb;

public sealed class PdbCompilationOptionsCustomDebugInfo : PdbCustomDebugInfo
{
	public override PdbCustomDebugInfoKind Kind => PdbCustomDebugInfoKind.CompilationOptions;

	public override Guid Guid => CustomDebugInfoGuids.CompilationOptions;

	public List<KeyValuePair<string, string>> Options { get; }

	public PdbCompilationOptionsCustomDebugInfo()
	{
		Options = new List<KeyValuePair<string, string>>();
	}
}
