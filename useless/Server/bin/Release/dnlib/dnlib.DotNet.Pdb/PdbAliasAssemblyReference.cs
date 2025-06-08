using System.Diagnostics;

namespace dnlib.DotNet.Pdb;

[DebuggerDisplay("{GetDebuggerString(),nq}")]
public sealed class PdbAliasAssemblyReference : PdbImport
{
	public sealed override PdbImportDefinitionKind Kind => PdbImportDefinitionKind.AliasAssemblyReference;

	public string Alias { get; set; }

	public AssemblyRef TargetAssembly { get; set; }

	public PdbAliasAssemblyReference()
	{
	}

	public PdbAliasAssemblyReference(string alias, AssemblyRef targetAssembly)
	{
		Alias = alias;
		TargetAssembly = targetAssembly;
	}

	internal sealed override void PreventNewClasses()
	{
	}

	private string GetDebuggerString()
	{
		return $"{Kind}: {Alias} = {TargetAssembly}";
	}
}
