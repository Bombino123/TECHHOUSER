using System.Diagnostics;

namespace dnlib.DotNet.Pdb;

[DebuggerDisplay("{GetDebuggerString(),nq}")]
public sealed class PdbImportAssemblyReferenceAlias : PdbImport
{
	public sealed override PdbImportDefinitionKind Kind => PdbImportDefinitionKind.ImportAssemblyReferenceAlias;

	public string Alias { get; set; }

	public PdbImportAssemblyReferenceAlias()
	{
	}

	public PdbImportAssemblyReferenceAlias(string alias)
	{
		Alias = alias;
	}

	internal sealed override void PreventNewClasses()
	{
	}

	private string GetDebuggerString()
	{
		return $"{Kind}: {Alias}";
	}
}
