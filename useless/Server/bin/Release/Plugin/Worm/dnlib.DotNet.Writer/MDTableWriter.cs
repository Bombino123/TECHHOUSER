using System.Runtime.InteropServices;
using dnlib.DotNet.MD;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public static class MDTableWriter
{
	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawModuleRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[1];
		ColumnInfo columnInfo2 = columns[2];
		ColumnInfo columnInfo3 = columns[3];
		ColumnInfo columnInfo4 = columns[4];
		StringsHeap stringsHeap = metadata.StringsHeap;
		for (int i = 0; i < table.Rows; i++)
		{
			RawModuleRow rawModuleRow = table[(uint)(i + 1)];
			writer.WriteUInt16(rawModuleRow.Generation);
			columnInfo.Write24(writer, stringsHeap.GetOffset(rawModuleRow.Name));
			columnInfo2.Write24(writer, rawModuleRow.Mvid);
			columnInfo3.Write24(writer, rawModuleRow.EncId);
			columnInfo4.Write24(writer, rawModuleRow.EncBaseId);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawTypeRefRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[0];
		ColumnInfo columnInfo2 = columns[1];
		ColumnInfo columnInfo3 = columns[2];
		StringsHeap stringsHeap = metadata.StringsHeap;
		for (int i = 0; i < table.Rows; i++)
		{
			RawTypeRefRow rawTypeRefRow = table[(uint)(i + 1)];
			columnInfo.Write24(writer, rawTypeRefRow.ResolutionScope);
			columnInfo2.Write24(writer, stringsHeap.GetOffset(rawTypeRefRow.Name));
			columnInfo3.Write24(writer, stringsHeap.GetOffset(rawTypeRefRow.Namespace));
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawTypeDefRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[1];
		ColumnInfo columnInfo2 = columns[2];
		ColumnInfo columnInfo3 = columns[3];
		ColumnInfo columnInfo4 = columns[4];
		ColumnInfo columnInfo5 = columns[5];
		StringsHeap stringsHeap = metadata.StringsHeap;
		for (int i = 0; i < table.Rows; i++)
		{
			RawTypeDefRow rawTypeDefRow = table[(uint)(i + 1)];
			writer.WriteUInt32(rawTypeDefRow.Flags);
			columnInfo.Write24(writer, stringsHeap.GetOffset(rawTypeDefRow.Name));
			columnInfo2.Write24(writer, stringsHeap.GetOffset(rawTypeDefRow.Namespace));
			columnInfo3.Write24(writer, rawTypeDefRow.Extends);
			columnInfo4.Write24(writer, rawTypeDefRow.FieldList);
			columnInfo5.Write24(writer, rawTypeDefRow.MethodList);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawFieldPtrRow> table)
	{
		ColumnInfo columnInfo = table.TableInfo.Columns[0];
		for (int i = 0; i < table.Rows; i++)
		{
			columnInfo.Write24(writer, table[(uint)(i + 1)].Field);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawFieldRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[1];
		ColumnInfo columnInfo2 = columns[2];
		StringsHeap stringsHeap = metadata.StringsHeap;
		for (int i = 0; i < table.Rows; i++)
		{
			RawFieldRow rawFieldRow = table[(uint)(i + 1)];
			writer.WriteUInt16(rawFieldRow.Flags);
			columnInfo.Write24(writer, stringsHeap.GetOffset(rawFieldRow.Name));
			columnInfo2.Write24(writer, rawFieldRow.Signature);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawMethodPtrRow> table)
	{
		ColumnInfo columnInfo = table.TableInfo.Columns[0];
		for (int i = 0; i < table.Rows; i++)
		{
			columnInfo.Write24(writer, table[(uint)(i + 1)].Method);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawMethodRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[3];
		ColumnInfo columnInfo2 = columns[4];
		ColumnInfo columnInfo3 = columns[5];
		StringsHeap stringsHeap = metadata.StringsHeap;
		for (int i = 0; i < table.Rows; i++)
		{
			RawMethodRow rawMethodRow = table[(uint)(i + 1)];
			writer.WriteUInt32(rawMethodRow.RVA);
			writer.WriteUInt16(rawMethodRow.ImplFlags);
			writer.WriteUInt16(rawMethodRow.Flags);
			columnInfo.Write24(writer, stringsHeap.GetOffset(rawMethodRow.Name));
			columnInfo2.Write24(writer, rawMethodRow.Signature);
			columnInfo3.Write24(writer, rawMethodRow.ParamList);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawParamPtrRow> table)
	{
		ColumnInfo columnInfo = table.TableInfo.Columns[0];
		for (int i = 0; i < table.Rows; i++)
		{
			columnInfo.Write24(writer, table[(uint)(i + 1)].Param);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawParamRow> table)
	{
		ColumnInfo columnInfo = table.TableInfo.Columns[2];
		StringsHeap stringsHeap = metadata.StringsHeap;
		for (int i = 0; i < table.Rows; i++)
		{
			RawParamRow rawParamRow = table[(uint)(i + 1)];
			writer.WriteUInt16(rawParamRow.Flags);
			writer.WriteUInt16(rawParamRow.Sequence);
			columnInfo.Write24(writer, stringsHeap.GetOffset(rawParamRow.Name));
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawInterfaceImplRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[0];
		ColumnInfo columnInfo2 = columns[1];
		for (int i = 0; i < table.Rows; i++)
		{
			RawInterfaceImplRow rawInterfaceImplRow = table[(uint)(i + 1)];
			columnInfo.Write24(writer, rawInterfaceImplRow.Class);
			columnInfo2.Write24(writer, rawInterfaceImplRow.Interface);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawMemberRefRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[0];
		ColumnInfo columnInfo2 = columns[1];
		ColumnInfo columnInfo3 = columns[2];
		StringsHeap stringsHeap = metadata.StringsHeap;
		for (int i = 0; i < table.Rows; i++)
		{
			RawMemberRefRow rawMemberRefRow = table[(uint)(i + 1)];
			columnInfo.Write24(writer, rawMemberRefRow.Class);
			columnInfo2.Write24(writer, stringsHeap.GetOffset(rawMemberRefRow.Name));
			columnInfo3.Write24(writer, rawMemberRefRow.Signature);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawConstantRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[2];
		ColumnInfo columnInfo2 = columns[3];
		for (int i = 0; i < table.Rows; i++)
		{
			RawConstantRow rawConstantRow = table[(uint)(i + 1)];
			writer.WriteByte(rawConstantRow.Type);
			writer.WriteByte(rawConstantRow.Padding);
			columnInfo.Write24(writer, rawConstantRow.Parent);
			columnInfo2.Write24(writer, rawConstantRow.Value);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawCustomAttributeRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[0];
		ColumnInfo columnInfo2 = columns[1];
		ColumnInfo columnInfo3 = columns[2];
		for (int i = 0; i < table.Rows; i++)
		{
			RawCustomAttributeRow rawCustomAttributeRow = table[(uint)(i + 1)];
			columnInfo.Write24(writer, rawCustomAttributeRow.Parent);
			columnInfo2.Write24(writer, rawCustomAttributeRow.Type);
			columnInfo3.Write24(writer, rawCustomAttributeRow.Value);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawFieldMarshalRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[0];
		ColumnInfo columnInfo2 = columns[1];
		for (int i = 0; i < table.Rows; i++)
		{
			RawFieldMarshalRow rawFieldMarshalRow = table[(uint)(i + 1)];
			columnInfo.Write24(writer, rawFieldMarshalRow.Parent);
			columnInfo2.Write24(writer, rawFieldMarshalRow.NativeType);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawDeclSecurityRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[1];
		ColumnInfo columnInfo2 = columns[2];
		for (int i = 0; i < table.Rows; i++)
		{
			RawDeclSecurityRow rawDeclSecurityRow = table[(uint)(i + 1)];
			writer.WriteInt16(rawDeclSecurityRow.Action);
			columnInfo.Write24(writer, rawDeclSecurityRow.Parent);
			columnInfo2.Write24(writer, rawDeclSecurityRow.PermissionSet);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawClassLayoutRow> table)
	{
		ColumnInfo columnInfo = table.TableInfo.Columns[2];
		for (int i = 0; i < table.Rows; i++)
		{
			RawClassLayoutRow rawClassLayoutRow = table[(uint)(i + 1)];
			writer.WriteUInt16(rawClassLayoutRow.PackingSize);
			writer.WriteUInt32(rawClassLayoutRow.ClassSize);
			columnInfo.Write24(writer, rawClassLayoutRow.Parent);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawFieldLayoutRow> table)
	{
		ColumnInfo columnInfo = table.TableInfo.Columns[1];
		for (int i = 0; i < table.Rows; i++)
		{
			RawFieldLayoutRow rawFieldLayoutRow = table[(uint)(i + 1)];
			writer.WriteUInt32(rawFieldLayoutRow.OffSet);
			columnInfo.Write24(writer, rawFieldLayoutRow.Field);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawStandAloneSigRow> table)
	{
		ColumnInfo columnInfo = table.TableInfo.Columns[0];
		for (int i = 0; i < table.Rows; i++)
		{
			columnInfo.Write24(writer, table[(uint)(i + 1)].Signature);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawEventMapRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[0];
		ColumnInfo columnInfo2 = columns[1];
		for (int i = 0; i < table.Rows; i++)
		{
			RawEventMapRow rawEventMapRow = table[(uint)(i + 1)];
			columnInfo.Write24(writer, rawEventMapRow.Parent);
			columnInfo2.Write24(writer, rawEventMapRow.EventList);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawEventPtrRow> table)
	{
		ColumnInfo columnInfo = table.TableInfo.Columns[0];
		for (int i = 0; i < table.Rows; i++)
		{
			columnInfo.Write24(writer, table[(uint)(i + 1)].Event);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawEventRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[1];
		ColumnInfo columnInfo2 = columns[2];
		StringsHeap stringsHeap = metadata.StringsHeap;
		for (int i = 0; i < table.Rows; i++)
		{
			RawEventRow rawEventRow = table[(uint)(i + 1)];
			writer.WriteUInt16(rawEventRow.EventFlags);
			columnInfo.Write24(writer, stringsHeap.GetOffset(rawEventRow.Name));
			columnInfo2.Write24(writer, rawEventRow.EventType);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawPropertyMapRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[0];
		ColumnInfo columnInfo2 = columns[1];
		for (int i = 0; i < table.Rows; i++)
		{
			RawPropertyMapRow rawPropertyMapRow = table[(uint)(i + 1)];
			columnInfo.Write24(writer, rawPropertyMapRow.Parent);
			columnInfo2.Write24(writer, rawPropertyMapRow.PropertyList);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawPropertyPtrRow> table)
	{
		ColumnInfo columnInfo = table.TableInfo.Columns[0];
		for (int i = 0; i < table.Rows; i++)
		{
			columnInfo.Write24(writer, table[(uint)(i + 1)].Property);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawPropertyRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[1];
		ColumnInfo columnInfo2 = columns[2];
		StringsHeap stringsHeap = metadata.StringsHeap;
		for (int i = 0; i < table.Rows; i++)
		{
			RawPropertyRow rawPropertyRow = table[(uint)(i + 1)];
			writer.WriteUInt16(rawPropertyRow.PropFlags);
			columnInfo.Write24(writer, stringsHeap.GetOffset(rawPropertyRow.Name));
			columnInfo2.Write24(writer, rawPropertyRow.Type);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawMethodSemanticsRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[1];
		ColumnInfo columnInfo2 = columns[2];
		for (int i = 0; i < table.Rows; i++)
		{
			RawMethodSemanticsRow rawMethodSemanticsRow = table[(uint)(i + 1)];
			writer.WriteUInt16(rawMethodSemanticsRow.Semantic);
			columnInfo.Write24(writer, rawMethodSemanticsRow.Method);
			columnInfo2.Write24(writer, rawMethodSemanticsRow.Association);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawMethodImplRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[0];
		ColumnInfo columnInfo2 = columns[1];
		ColumnInfo columnInfo3 = columns[2];
		for (int i = 0; i < table.Rows; i++)
		{
			RawMethodImplRow rawMethodImplRow = table[(uint)(i + 1)];
			columnInfo.Write24(writer, rawMethodImplRow.Class);
			columnInfo2.Write24(writer, rawMethodImplRow.MethodBody);
			columnInfo3.Write24(writer, rawMethodImplRow.MethodDeclaration);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawModuleRefRow> table)
	{
		ColumnInfo columnInfo = table.TableInfo.Columns[0];
		StringsHeap stringsHeap = metadata.StringsHeap;
		for (int i = 0; i < table.Rows; i++)
		{
			columnInfo.Write24(writer, stringsHeap.GetOffset(table[(uint)(i + 1)].Name));
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawTypeSpecRow> table)
	{
		ColumnInfo columnInfo = table.TableInfo.Columns[0];
		for (int i = 0; i < table.Rows; i++)
		{
			columnInfo.Write24(writer, table[(uint)(i + 1)].Signature);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawImplMapRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[1];
		ColumnInfo columnInfo2 = columns[2];
		ColumnInfo columnInfo3 = columns[3];
		StringsHeap stringsHeap = metadata.StringsHeap;
		for (int i = 0; i < table.Rows; i++)
		{
			RawImplMapRow rawImplMapRow = table[(uint)(i + 1)];
			writer.WriteUInt16(rawImplMapRow.MappingFlags);
			columnInfo.Write24(writer, rawImplMapRow.MemberForwarded);
			columnInfo2.Write24(writer, stringsHeap.GetOffset(rawImplMapRow.ImportName));
			columnInfo3.Write24(writer, rawImplMapRow.ImportScope);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawFieldRVARow> table)
	{
		ColumnInfo columnInfo = table.TableInfo.Columns[1];
		for (int i = 0; i < table.Rows; i++)
		{
			RawFieldRVARow rawFieldRVARow = table[(uint)(i + 1)];
			writer.WriteUInt32(rawFieldRVARow.RVA);
			columnInfo.Write24(writer, rawFieldRVARow.Field);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawENCLogRow> table)
	{
		for (int i = 0; i < table.Rows; i++)
		{
			RawENCLogRow rawENCLogRow = table[(uint)(i + 1)];
			writer.WriteUInt32(rawENCLogRow.Token);
			writer.WriteUInt32(rawENCLogRow.FuncCode);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawENCMapRow> table)
	{
		for (int i = 0; i < table.Rows; i++)
		{
			writer.WriteUInt32(table[(uint)(i + 1)].Token);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawAssemblyRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[6];
		ColumnInfo columnInfo2 = columns[7];
		ColumnInfo columnInfo3 = columns[8];
		StringsHeap stringsHeap = metadata.StringsHeap;
		for (int i = 0; i < table.Rows; i++)
		{
			RawAssemblyRow rawAssemblyRow = table[(uint)(i + 1)];
			writer.WriteUInt32(rawAssemblyRow.HashAlgId);
			writer.WriteUInt16(rawAssemblyRow.MajorVersion);
			writer.WriteUInt16(rawAssemblyRow.MinorVersion);
			writer.WriteUInt16(rawAssemblyRow.BuildNumber);
			writer.WriteUInt16(rawAssemblyRow.RevisionNumber);
			writer.WriteUInt32(rawAssemblyRow.Flags);
			columnInfo.Write24(writer, rawAssemblyRow.PublicKey);
			columnInfo2.Write24(writer, stringsHeap.GetOffset(rawAssemblyRow.Name));
			columnInfo3.Write24(writer, stringsHeap.GetOffset(rawAssemblyRow.Locale));
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawAssemblyProcessorRow> table)
	{
		for (int i = 0; i < table.Rows; i++)
		{
			writer.WriteUInt32(table[(uint)(i + 1)].Processor);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawAssemblyOSRow> table)
	{
		for (int i = 0; i < table.Rows; i++)
		{
			RawAssemblyOSRow rawAssemblyOSRow = table[(uint)(i + 1)];
			writer.WriteUInt32(rawAssemblyOSRow.OSPlatformId);
			writer.WriteUInt32(rawAssemblyOSRow.OSMajorVersion);
			writer.WriteUInt32(rawAssemblyOSRow.OSMinorVersion);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawAssemblyRefRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[5];
		ColumnInfo columnInfo2 = columns[6];
		ColumnInfo columnInfo3 = columns[7];
		ColumnInfo columnInfo4 = columns[8];
		StringsHeap stringsHeap = metadata.StringsHeap;
		for (int i = 0; i < table.Rows; i++)
		{
			RawAssemblyRefRow rawAssemblyRefRow = table[(uint)(i + 1)];
			writer.WriteUInt16(rawAssemblyRefRow.MajorVersion);
			writer.WriteUInt16(rawAssemblyRefRow.MinorVersion);
			writer.WriteUInt16(rawAssemblyRefRow.BuildNumber);
			writer.WriteUInt16(rawAssemblyRefRow.RevisionNumber);
			writer.WriteUInt32(rawAssemblyRefRow.Flags);
			columnInfo.Write24(writer, rawAssemblyRefRow.PublicKeyOrToken);
			columnInfo2.Write24(writer, stringsHeap.GetOffset(rawAssemblyRefRow.Name));
			columnInfo3.Write24(writer, stringsHeap.GetOffset(rawAssemblyRefRow.Locale));
			columnInfo4.Write24(writer, rawAssemblyRefRow.HashValue);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawAssemblyRefProcessorRow> table)
	{
		ColumnInfo columnInfo = table.TableInfo.Columns[1];
		for (int i = 0; i < table.Rows; i++)
		{
			RawAssemblyRefProcessorRow rawAssemblyRefProcessorRow = table[(uint)(i + 1)];
			writer.WriteUInt32(rawAssemblyRefProcessorRow.Processor);
			columnInfo.Write24(writer, rawAssemblyRefProcessorRow.AssemblyRef);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawAssemblyRefOSRow> table)
	{
		ColumnInfo columnInfo = table.TableInfo.Columns[3];
		for (int i = 0; i < table.Rows; i++)
		{
			RawAssemblyRefOSRow rawAssemblyRefOSRow = table[(uint)(i + 1)];
			writer.WriteUInt32(rawAssemblyRefOSRow.OSPlatformId);
			writer.WriteUInt32(rawAssemblyRefOSRow.OSMajorVersion);
			writer.WriteUInt32(rawAssemblyRefOSRow.OSMinorVersion);
			columnInfo.Write24(writer, rawAssemblyRefOSRow.AssemblyRef);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawFileRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[1];
		ColumnInfo columnInfo2 = columns[2];
		StringsHeap stringsHeap = metadata.StringsHeap;
		for (int i = 0; i < table.Rows; i++)
		{
			RawFileRow rawFileRow = table[(uint)(i + 1)];
			writer.WriteUInt32(rawFileRow.Flags);
			columnInfo.Write24(writer, stringsHeap.GetOffset(rawFileRow.Name));
			columnInfo2.Write24(writer, rawFileRow.HashValue);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawExportedTypeRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[2];
		ColumnInfo columnInfo2 = columns[3];
		ColumnInfo columnInfo3 = columns[4];
		StringsHeap stringsHeap = metadata.StringsHeap;
		for (int i = 0; i < table.Rows; i++)
		{
			RawExportedTypeRow rawExportedTypeRow = table[(uint)(i + 1)];
			writer.WriteUInt32(rawExportedTypeRow.Flags);
			writer.WriteUInt32(rawExportedTypeRow.TypeDefId);
			columnInfo.Write24(writer, stringsHeap.GetOffset(rawExportedTypeRow.TypeName));
			columnInfo2.Write24(writer, stringsHeap.GetOffset(rawExportedTypeRow.TypeNamespace));
			columnInfo3.Write24(writer, rawExportedTypeRow.Implementation);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawManifestResourceRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[2];
		ColumnInfo columnInfo2 = columns[3];
		StringsHeap stringsHeap = metadata.StringsHeap;
		for (int i = 0; i < table.Rows; i++)
		{
			RawManifestResourceRow rawManifestResourceRow = table[(uint)(i + 1)];
			writer.WriteUInt32(rawManifestResourceRow.Offset);
			writer.WriteUInt32(rawManifestResourceRow.Flags);
			columnInfo.Write24(writer, stringsHeap.GetOffset(rawManifestResourceRow.Name));
			columnInfo2.Write24(writer, rawManifestResourceRow.Implementation);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawNestedClassRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[0];
		ColumnInfo columnInfo2 = columns[1];
		for (int i = 0; i < table.Rows; i++)
		{
			RawNestedClassRow rawNestedClassRow = table[(uint)(i + 1)];
			columnInfo.Write24(writer, rawNestedClassRow.NestedClass);
			columnInfo2.Write24(writer, rawNestedClassRow.EnclosingClass);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawGenericParamRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[2];
		ColumnInfo columnInfo2 = columns[3];
		StringsHeap stringsHeap = metadata.StringsHeap;
		if (columns.Length >= 5)
		{
			ColumnInfo columnInfo3 = columns[4];
			for (int i = 0; i < table.Rows; i++)
			{
				RawGenericParamRow rawGenericParamRow = table[(uint)(i + 1)];
				writer.WriteUInt16(rawGenericParamRow.Number);
				writer.WriteUInt16(rawGenericParamRow.Flags);
				columnInfo.Write24(writer, rawGenericParamRow.Owner);
				columnInfo2.Write24(writer, stringsHeap.GetOffset(rawGenericParamRow.Name));
				columnInfo3.Write24(writer, rawGenericParamRow.Kind);
			}
		}
		else
		{
			for (int j = 0; j < table.Rows; j++)
			{
				RawGenericParamRow rawGenericParamRow2 = table[(uint)(j + 1)];
				writer.WriteUInt16(rawGenericParamRow2.Number);
				writer.WriteUInt16(rawGenericParamRow2.Flags);
				columnInfo.Write24(writer, rawGenericParamRow2.Owner);
				columnInfo2.Write24(writer, stringsHeap.GetOffset(rawGenericParamRow2.Name));
			}
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawMethodSpecRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[0];
		ColumnInfo columnInfo2 = columns[1];
		for (int i = 0; i < table.Rows; i++)
		{
			RawMethodSpecRow rawMethodSpecRow = table[(uint)(i + 1)];
			columnInfo.Write24(writer, rawMethodSpecRow.Method);
			columnInfo2.Write24(writer, rawMethodSpecRow.Instantiation);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawGenericParamConstraintRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[0];
		ColumnInfo columnInfo2 = columns[1];
		for (int i = 0; i < table.Rows; i++)
		{
			RawGenericParamConstraintRow rawGenericParamConstraintRow = table[(uint)(i + 1)];
			columnInfo.Write24(writer, rawGenericParamConstraintRow.Owner);
			columnInfo2.Write24(writer, rawGenericParamConstraintRow.Constraint);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawDocumentRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[0];
		ColumnInfo columnInfo2 = columns[1];
		ColumnInfo columnInfo3 = columns[2];
		ColumnInfo columnInfo4 = columns[3];
		for (int i = 0; i < table.Rows; i++)
		{
			RawDocumentRow rawDocumentRow = table[(uint)(i + 1)];
			columnInfo.Write24(writer, rawDocumentRow.Name);
			columnInfo2.Write24(writer, rawDocumentRow.HashAlgorithm);
			columnInfo3.Write24(writer, rawDocumentRow.Hash);
			columnInfo4.Write24(writer, rawDocumentRow.Language);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawMethodDebugInformationRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[0];
		ColumnInfo columnInfo2 = columns[1];
		for (int i = 0; i < table.Rows; i++)
		{
			RawMethodDebugInformationRow rawMethodDebugInformationRow = table[(uint)(i + 1)];
			columnInfo.Write24(writer, rawMethodDebugInformationRow.Document);
			columnInfo2.Write24(writer, rawMethodDebugInformationRow.SequencePoints);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawLocalScopeRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[0];
		ColumnInfo columnInfo2 = columns[1];
		ColumnInfo columnInfo3 = columns[2];
		ColumnInfo columnInfo4 = columns[3];
		for (int i = 0; i < table.Rows; i++)
		{
			RawLocalScopeRow rawLocalScopeRow = table[(uint)(i + 1)];
			columnInfo.Write24(writer, rawLocalScopeRow.Method);
			columnInfo2.Write24(writer, rawLocalScopeRow.ImportScope);
			columnInfo3.Write24(writer, rawLocalScopeRow.VariableList);
			columnInfo4.Write24(writer, rawLocalScopeRow.ConstantList);
			writer.WriteUInt32(rawLocalScopeRow.StartOffset);
			writer.WriteUInt32(rawLocalScopeRow.Length);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawLocalVariableRow> table)
	{
		ColumnInfo columnInfo = table.TableInfo.Columns[2];
		StringsHeap stringsHeap = metadata.StringsHeap;
		for (int i = 0; i < table.Rows; i++)
		{
			RawLocalVariableRow rawLocalVariableRow = table[(uint)(i + 1)];
			writer.WriteUInt16(rawLocalVariableRow.Attributes);
			writer.WriteUInt16(rawLocalVariableRow.Index);
			columnInfo.Write24(writer, stringsHeap.GetOffset(rawLocalVariableRow.Name));
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawLocalConstantRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[0];
		ColumnInfo columnInfo2 = columns[1];
		StringsHeap stringsHeap = metadata.StringsHeap;
		for (int i = 0; i < table.Rows; i++)
		{
			RawLocalConstantRow rawLocalConstantRow = table[(uint)(i + 1)];
			columnInfo.Write24(writer, stringsHeap.GetOffset(rawLocalConstantRow.Name));
			columnInfo2.Write24(writer, rawLocalConstantRow.Signature);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawImportScopeRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[0];
		ColumnInfo columnInfo2 = columns[1];
		for (int i = 0; i < table.Rows; i++)
		{
			RawImportScopeRow rawImportScopeRow = table[(uint)(i + 1)];
			columnInfo.Write24(writer, rawImportScopeRow.Parent);
			columnInfo2.Write24(writer, rawImportScopeRow.Imports);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawStateMachineMethodRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[0];
		ColumnInfo columnInfo2 = columns[1];
		for (int i = 0; i < table.Rows; i++)
		{
			RawStateMachineMethodRow rawStateMachineMethodRow = table[(uint)(i + 1)];
			columnInfo.Write24(writer, rawStateMachineMethodRow.MoveNextMethod);
			columnInfo2.Write24(writer, rawStateMachineMethodRow.KickoffMethod);
		}
	}

	public static void Write(this DataWriter writer, Metadata metadata, MDTable<RawCustomDebugInformationRow> table)
	{
		ColumnInfo[] columns = table.TableInfo.Columns;
		ColumnInfo columnInfo = columns[0];
		ColumnInfo columnInfo2 = columns[1];
		ColumnInfo columnInfo3 = columns[2];
		for (int i = 0; i < table.Rows; i++)
		{
			RawCustomDebugInformationRow rawCustomDebugInformationRow = table[(uint)(i + 1)];
			columnInfo.Write24(writer, rawCustomDebugInformationRow.Parent);
			columnInfo2.Write24(writer, rawCustomDebugInformationRow.Kind);
			columnInfo3.Write24(writer, rawCustomDebugInformationRow.Value);
		}
	}
}
