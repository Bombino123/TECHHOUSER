using System.Diagnostics;

namespace dnlib.DotNet.Pdb;

[DebuggerDisplay("{GetDebuggerString(),nq}")]
public sealed class PdbAliasNamespace : PdbImport
{
	public sealed override PdbImportDefinitionKind Kind => PdbImportDefinitionKind.AliasNamespace;

	public string Alias { get; set; }

	public string TargetNamespace { get; set; }

	public PdbAliasNamespace()
	{
	}

	public PdbAliasNamespace(string alias, string targetNamespace)
	{
		Alias = alias;
		TargetNamespace = targetNamespace;
	}

	internal sealed override void PreventNewClasses()
	{
	}

	private string GetDebuggerString()
	{
		return $"{Kind}: {Alias} = {TargetNamespace}";
	}
}
