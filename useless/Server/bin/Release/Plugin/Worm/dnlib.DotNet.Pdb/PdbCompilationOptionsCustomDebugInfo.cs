using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
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
