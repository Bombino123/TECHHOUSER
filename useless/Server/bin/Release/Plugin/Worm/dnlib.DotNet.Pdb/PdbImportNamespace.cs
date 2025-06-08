using System.Diagnostics;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[DebuggerDisplay("{GetDebuggerString(),nq}")]
[ComVisible(true)]
public sealed class PdbImportNamespace : PdbImport
{
	public sealed override PdbImportDefinitionKind Kind => PdbImportDefinitionKind.ImportNamespace;

	public string TargetNamespace { get; set; }

	public PdbImportNamespace()
	{
	}

	public PdbImportNamespace(string targetNamespace)
	{
		TargetNamespace = targetNamespace;
	}

	internal sealed override void PreventNewClasses()
	{
	}

	private string GetDebuggerString()
	{
		return $"{Kind}: {TargetNamespace}";
	}
}
