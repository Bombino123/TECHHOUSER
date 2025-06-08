using System.Collections.Generic;
using System.Text;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;

namespace dnlib.DotNet.Pdb.Portable;

internal readonly struct ImportScopeBlobWriter
{
	private readonly IWriterError helper;

	private readonly dnlib.DotNet.Writer.Metadata systemMetadata;

	private readonly BlobHeap blobHeap;

	private ImportScopeBlobWriter(IWriterError helper, dnlib.DotNet.Writer.Metadata systemMetadata, BlobHeap blobHeap)
	{
		this.helper = helper;
		this.systemMetadata = systemMetadata;
		this.blobHeap = blobHeap;
	}

	public static void Write(IWriterError helper, dnlib.DotNet.Writer.Metadata systemMetadata, DataWriter writer, BlobHeap blobHeap, IList<PdbImport> imports)
	{
		new ImportScopeBlobWriter(helper, systemMetadata, blobHeap).Write(writer, imports);
	}

	private uint WriteUTF8(string s)
	{
		if (s == null)
		{
			helper.Error("String is null");
			s = string.Empty;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		return blobHeap.Add(bytes);
	}

	private void Write(DataWriter writer, IList<PdbImport> imports)
	{
		int count = imports.Count;
		for (int i = 0; i < count; i++)
		{
			PdbImport pdbImport = imports[i];
			if (!ImportDefinitionKindUtils.ToImportDefinitionKind(pdbImport.Kind, out var rawKind))
			{
				helper.Error2("Unknown import definition kind: {0}.", pdbImport.Kind);
				break;
			}
			writer.WriteCompressedUInt32(rawKind);
			switch (pdbImport.Kind)
			{
			case PdbImportDefinitionKind.ImportNamespace:
				writer.WriteCompressedUInt32(WriteUTF8(((PdbImportNamespace)pdbImport).TargetNamespace));
				break;
			case PdbImportDefinitionKind.ImportAssemblyNamespace:
				writer.WriteCompressedUInt32(systemMetadata.GetToken(((PdbImportAssemblyNamespace)pdbImport).TargetAssembly).Rid);
				writer.WriteCompressedUInt32(WriteUTF8(((PdbImportAssemblyNamespace)pdbImport).TargetNamespace));
				break;
			case PdbImportDefinitionKind.ImportType:
				writer.WriteCompressedUInt32(GetTypeDefOrRefEncodedToken(((PdbImportType)pdbImport).TargetType));
				break;
			case PdbImportDefinitionKind.ImportXmlNamespace:
				writer.WriteCompressedUInt32(WriteUTF8(((PdbImportXmlNamespace)pdbImport).Alias));
				writer.WriteCompressedUInt32(WriteUTF8(((PdbImportXmlNamespace)pdbImport).TargetNamespace));
				break;
			case PdbImportDefinitionKind.ImportAssemblyReferenceAlias:
				writer.WriteCompressedUInt32(WriteUTF8(((PdbImportAssemblyReferenceAlias)pdbImport).Alias));
				break;
			case PdbImportDefinitionKind.AliasAssemblyReference:
				writer.WriteCompressedUInt32(WriteUTF8(((PdbAliasAssemblyReference)pdbImport).Alias));
				writer.WriteCompressedUInt32(systemMetadata.GetToken(((PdbAliasAssemblyReference)pdbImport).TargetAssembly).Rid);
				break;
			case PdbImportDefinitionKind.AliasNamespace:
				writer.WriteCompressedUInt32(WriteUTF8(((PdbAliasNamespace)pdbImport).Alias));
				writer.WriteCompressedUInt32(WriteUTF8(((PdbAliasNamespace)pdbImport).TargetNamespace));
				break;
			case PdbImportDefinitionKind.AliasAssemblyNamespace:
				writer.WriteCompressedUInt32(WriteUTF8(((PdbAliasAssemblyNamespace)pdbImport).Alias));
				writer.WriteCompressedUInt32(systemMetadata.GetToken(((PdbAliasAssemblyNamespace)pdbImport).TargetAssembly).Rid);
				writer.WriteCompressedUInt32(WriteUTF8(((PdbAliasAssemblyNamespace)pdbImport).TargetNamespace));
				break;
			case PdbImportDefinitionKind.AliasType:
				writer.WriteCompressedUInt32(WriteUTF8(((PdbAliasType)pdbImport).Alias));
				writer.WriteCompressedUInt32(GetTypeDefOrRefEncodedToken(((PdbAliasType)pdbImport).TargetType));
				break;
			default:
				helper.Error2("Unknown import definition kind: {0}.", pdbImport.Kind);
				return;
			}
		}
	}

	private uint GetTypeDefOrRefEncodedToken(ITypeDefOrRef tdr)
	{
		if (tdr == null)
		{
			helper.Error("ITypeDefOrRef is null");
			return 0u;
		}
		MDToken token = systemMetadata.GetToken(tdr);
		if (CodedToken.TypeDefOrRef.Encode(token, out var codedToken))
		{
			return codedToken;
		}
		helper.Error2("Could not encode token 0x{0:X8}.", token.Raw);
		return 0u;
	}
}
