using System.Diagnostics;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[DebuggerDisplay("{GetDebuggerString(),nq}")]
[ComVisible(true)]
public sealed class PdbAliasAssemblyNamespace : PdbImport
{
	public sealed override PdbImportDefinitionKind Kind => PdbImportDefinitionKind.AliasAssemblyNamespace;

	public string Alias { get; set; }

	public AssemblyRef TargetAssembly { get; set; }

	public string TargetNamespace { get; set; }

	public PdbAliasAssemblyNamespace()
	{
	}

	public PdbAliasAssemblyNamespace(string alias, AssemblyRef targetAssembly, string targetNamespace)
	{
		Alias = alias;
		TargetAssembly = targetAssembly;
		TargetNamespace = targetNamespace;
	}

	internal sealed override void PreventNewClasses()
	{
	}

	private string GetDebuggerString()
	{
		return $"{Kind}: {Alias} = {TargetAssembly} {TargetNamespace}";
	}
}
