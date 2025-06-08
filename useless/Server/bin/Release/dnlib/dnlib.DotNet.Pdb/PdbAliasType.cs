using System.Diagnostics;

namespace dnlib.DotNet.Pdb;

[DebuggerDisplay("{GetDebuggerString(),nq}")]
public sealed class PdbAliasType : PdbImport
{
	public sealed override PdbImportDefinitionKind Kind => PdbImportDefinitionKind.AliasType;

	public string Alias { get; set; }

	public ITypeDefOrRef TargetType { get; set; }

	public PdbAliasType()
	{
	}

	public PdbAliasType(string alias, ITypeDefOrRef targetType)
	{
		Alias = alias;
		TargetType = targetType;
	}

	internal sealed override void PreventNewClasses()
	{
	}

	private string GetDebuggerString()
	{
		return $"{Kind}: {Alias} = {TargetType}";
	}
}
