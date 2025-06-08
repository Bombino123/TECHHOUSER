using System.Diagnostics;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[DebuggerDisplay("{GetDebuggerString(),nq}")]
[ComVisible(true)]
public sealed class PdbImportType : PdbImport
{
	public sealed override PdbImportDefinitionKind Kind => PdbImportDefinitionKind.ImportType;

	public ITypeDefOrRef TargetType { get; set; }

	public PdbImportType()
	{
	}

	public PdbImportType(ITypeDefOrRef targetType)
	{
		TargetType = targetType;
	}

	internal sealed override void PreventNewClasses()
	{
	}

	private string GetDebuggerString()
	{
		return $"{Kind}: {TargetType}";
	}
}
