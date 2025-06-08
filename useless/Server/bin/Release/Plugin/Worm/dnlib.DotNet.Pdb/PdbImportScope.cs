using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
public sealed class PdbImportScope : IHasCustomDebugInformation
{
	private readonly IList<PdbImport> imports = new List<PdbImport>();

	private readonly IList<PdbCustomDebugInfo> customDebugInfos = new List<PdbCustomDebugInfo>();

	public PdbImportScope Parent { get; set; }

	public IList<PdbImport> Imports => imports;

	public bool HasImports => imports.Count > 0;

	public int HasCustomDebugInformationTag => 26;

	public bool HasCustomDebugInfos => CustomDebugInfos.Count > 0;

	public IList<PdbCustomDebugInfo> CustomDebugInfos => customDebugInfos;
}
