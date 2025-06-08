using System.Diagnostics;

namespace dnlib.DotNet.Pdb;

[DebuggerDisplay("{GetDebuggerString(),nq}")]
public sealed class PdbImportXmlNamespace : PdbImport
{
	public sealed override PdbImportDefinitionKind Kind => PdbImportDefinitionKind.ImportXmlNamespace;

	public string Alias { get; set; }

	public string TargetNamespace { get; set; }

	public PdbImportXmlNamespace()
	{
	}

	public PdbImportXmlNamespace(string alias, string targetNamespace)
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
