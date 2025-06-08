namespace dnlib.DotNet.Pdb.Portable;

internal static class ImportDefinitionKindUtils
{
	public const PdbImportDefinitionKind UNKNOWN_IMPORT_KIND = (PdbImportDefinitionKind)(-1);

	public static PdbImportDefinitionKind ToPdbImportDefinitionKind(uint value)
	{
		return value switch
		{
			1u => PdbImportDefinitionKind.ImportNamespace, 
			2u => PdbImportDefinitionKind.ImportAssemblyNamespace, 
			3u => PdbImportDefinitionKind.ImportType, 
			4u => PdbImportDefinitionKind.ImportXmlNamespace, 
			5u => PdbImportDefinitionKind.ImportAssemblyReferenceAlias, 
			6u => PdbImportDefinitionKind.AliasAssemblyReference, 
			7u => PdbImportDefinitionKind.AliasNamespace, 
			8u => PdbImportDefinitionKind.AliasAssemblyNamespace, 
			9u => PdbImportDefinitionKind.AliasType, 
			_ => (PdbImportDefinitionKind)(-1), 
		};
	}

	public static bool ToImportDefinitionKind(PdbImportDefinitionKind kind, out uint rawKind)
	{
		switch (kind)
		{
		case PdbImportDefinitionKind.ImportNamespace:
			rawKind = 1u;
			return true;
		case PdbImportDefinitionKind.ImportAssemblyNamespace:
			rawKind = 2u;
			return true;
		case PdbImportDefinitionKind.ImportType:
			rawKind = 3u;
			return true;
		case PdbImportDefinitionKind.ImportXmlNamespace:
			rawKind = 4u;
			return true;
		case PdbImportDefinitionKind.ImportAssemblyReferenceAlias:
			rawKind = 5u;
			return true;
		case PdbImportDefinitionKind.AliasAssemblyReference:
			rawKind = 6u;
			return true;
		case PdbImportDefinitionKind.AliasNamespace:
			rawKind = 7u;
			return true;
		case PdbImportDefinitionKind.AliasAssemblyNamespace:
			rawKind = 8u;
			return true;
		case PdbImportDefinitionKind.AliasType:
			rawKind = 9u;
			return true;
		default:
			rawKind = uint.MaxValue;
			return false;
		}
	}
}
