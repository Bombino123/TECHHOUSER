using System.Diagnostics;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[DebuggerDisplay("{GetDebuggerString(),nq}")]
[ComVisible(true)]
public sealed class PdbImportAssemblyNamespace : PdbImport
{
	public sealed override PdbImportDefinitionKind Kind => PdbImportDefinitionKind.ImportAssemblyNamespace;

	public AssemblyRef TargetAssembly { get; set; }

	public string TargetNamespace { get; set; }

	public PdbImportAssemblyNamespace()
	{
	}

	public PdbImportAssemblyNamespace(AssemblyRef targetAssembly, string targetNamespace)
	{
		TargetAssembly = targetAssembly;
		TargetNamespace = targetNamespace;
	}

	internal sealed override void PreventNewClasses()
	{
	}

	private string GetDebuggerString()
	{
		return $"{Kind}: {TargetAssembly} {TargetNamespace}";
	}
}
