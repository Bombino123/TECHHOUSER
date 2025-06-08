using System.Collections.Generic;
using System.Diagnostics;
using dnlib.DotNet.Emit;

namespace dnlib.DotNet.Pdb;

[DebuggerDisplay("{Start} - {End}")]
public sealed class PdbScope : IHasCustomDebugInformation
{
	private readonly IList<PdbScope> scopes = new List<PdbScope>();

	private readonly IList<PdbLocal> locals = new List<PdbLocal>();

	private readonly IList<string> namespaces = new List<string>();

	private readonly IList<PdbConstant> constants = new List<PdbConstant>();

	private readonly IList<PdbCustomDebugInfo> customDebugInfos = new List<PdbCustomDebugInfo>();

	public Instruction Start { get; set; }

	public Instruction End { get; set; }

	public IList<PdbScope> Scopes => scopes;

	public bool HasScopes => scopes.Count > 0;

	public IList<PdbLocal> Variables => locals;

	public bool HasVariables => locals.Count > 0;

	public IList<string> Namespaces => namespaces;

	public bool HasNamespaces => namespaces.Count > 0;

	public PdbImportScope ImportScope { get; set; }

	public IList<PdbConstant> Constants => constants;

	public bool HasConstants => constants.Count > 0;

	public int HasCustomDebugInformationTag => 23;

	public bool HasCustomDebugInfos => CustomDebugInfos.Count > 0;

	public IList<PdbCustomDebugInfo> CustomDebugInfos => customDebugInfos;
}
