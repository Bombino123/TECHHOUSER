using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
public enum PdbImportDefinitionKind
{
	ImportNamespace,
	ImportAssemblyNamespace,
	ImportType,
	ImportXmlNamespace,
	ImportAssemblyReferenceAlias,
	AliasAssemblyReference,
	AliasNamespace,
	AliasAssemblyNamespace,
	AliasType
}
