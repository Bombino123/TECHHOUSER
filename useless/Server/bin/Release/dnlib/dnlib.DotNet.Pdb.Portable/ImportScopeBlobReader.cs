using System.Collections.Generic;
using dnlib.DotNet.MD;

namespace dnlib.DotNet.Pdb.Portable;

internal readonly struct ImportScopeBlobReader
{
	private readonly ModuleDef module;

	private readonly BlobStream blobStream;

	public ImportScopeBlobReader(ModuleDef module, BlobStream blobStream)
	{
		this.module = module;
		this.blobStream = blobStream;
	}

	public void Read(uint imports, IList<PdbImport> result)
	{
		if (imports == 0 || !blobStream.TryCreateReader(imports, out var reader))
		{
			return;
		}
		while (reader.Position < reader.Length)
		{
			PdbImport pdbImport;
			switch (ImportDefinitionKindUtils.ToPdbImportDefinitionKind(reader.ReadCompressedUInt32()))
			{
			case PdbImportDefinitionKind.ImportNamespace:
			{
				string targetNamespace = ReadUTF8(reader.ReadCompressedUInt32());
				pdbImport = new PdbImportNamespace(targetNamespace);
				break;
			}
			case PdbImportDefinitionKind.ImportAssemblyNamespace:
			{
				AssemblyRef targetAssembly = TryReadAssemblyRef(reader.ReadCompressedUInt32());
				string targetNamespace = ReadUTF8(reader.ReadCompressedUInt32());
				pdbImport = new PdbImportAssemblyNamespace(targetAssembly, targetNamespace);
				break;
			}
			case PdbImportDefinitionKind.ImportType:
			{
				ITypeDefOrRef targetType = TryReadType(reader.ReadCompressedUInt32());
				pdbImport = new PdbImportType(targetType);
				break;
			}
			case PdbImportDefinitionKind.ImportXmlNamespace:
			{
				string alias5 = ReadUTF8(reader.ReadCompressedUInt32());
				string targetNamespace = ReadUTF8(reader.ReadCompressedUInt32());
				pdbImport = new PdbImportXmlNamespace(alias5, targetNamespace);
				break;
			}
			case PdbImportDefinitionKind.ImportAssemblyReferenceAlias:
				pdbImport = new PdbImportAssemblyReferenceAlias(ReadUTF8(reader.ReadCompressedUInt32()));
				break;
			case PdbImportDefinitionKind.AliasAssemblyReference:
			{
				string alias4 = ReadUTF8(reader.ReadCompressedUInt32());
				AssemblyRef targetAssembly = TryReadAssemblyRef(reader.ReadCompressedUInt32());
				pdbImport = new PdbAliasAssemblyReference(alias4, targetAssembly);
				break;
			}
			case PdbImportDefinitionKind.AliasNamespace:
			{
				string alias3 = ReadUTF8(reader.ReadCompressedUInt32());
				string targetNamespace = ReadUTF8(reader.ReadCompressedUInt32());
				pdbImport = new PdbAliasNamespace(alias3, targetNamespace);
				break;
			}
			case PdbImportDefinitionKind.AliasAssemblyNamespace:
			{
				string alias2 = ReadUTF8(reader.ReadCompressedUInt32());
				AssemblyRef targetAssembly = TryReadAssemblyRef(reader.ReadCompressedUInt32());
				string targetNamespace = ReadUTF8(reader.ReadCompressedUInt32());
				pdbImport = new PdbAliasAssemblyNamespace(alias2, targetAssembly, targetNamespace);
				break;
			}
			case PdbImportDefinitionKind.AliasType:
			{
				string alias = ReadUTF8(reader.ReadCompressedUInt32());
				ITypeDefOrRef targetType = TryReadType(reader.ReadCompressedUInt32());
				pdbImport = new PdbAliasType(alias, targetType);
				break;
			}
			case (PdbImportDefinitionKind)(-1):
				pdbImport = null;
				break;
			default:
				pdbImport = null;
				break;
			}
			if (pdbImport != null)
			{
				result.Add(pdbImport);
			}
		}
	}

	private ITypeDefOrRef TryReadType(uint codedToken)
	{
		if (!CodedToken.TypeDefOrRef.Decode(codedToken, out uint token))
		{
			return null;
		}
		return module.ResolveToken(token) as ITypeDefOrRef;
	}

	private AssemblyRef TryReadAssemblyRef(uint rid)
	{
		return module.ResolveToken(587202560 + rid) as AssemblyRef;
	}

	private string ReadUTF8(uint offset)
	{
		if (!blobStream.TryCreateReader(offset, out var reader))
		{
			return string.Empty;
		}
		return reader.ReadUtf8String((int)reader.BytesLeft);
	}
}
