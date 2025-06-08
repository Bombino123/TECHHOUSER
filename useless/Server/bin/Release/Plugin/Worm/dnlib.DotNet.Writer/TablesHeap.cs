using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using dnlib.DotNet.MD;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public sealed class TablesHeap : IHeap, IChunk
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct RawDummyRow
	{
		private sealed class RawDummyRowEqualityComparer : IEqualityComparer<RawDummyRow>
		{
			public bool Equals(RawDummyRow x, RawDummyRow y)
			{
				throw new NotSupportedException();
			}

			public int GetHashCode(RawDummyRow obj)
			{
				throw new NotSupportedException();
			}
		}

		public static readonly IEqualityComparer<RawDummyRow> Comparer = new RawDummyRowEqualityComparer();
	}

	private uint length;

	private byte majorVersion;

	private byte minorVersion;

	private bool bigStrings;

	private bool bigGuid;

	private bool bigBlob;

	private bool hasDeletedRows;

	private readonly Metadata metadata;

	private readonly TablesHeapOptions options;

	private FileOffset offset;

	private RVA rva;

	public readonly MDTable<RawModuleRow> ModuleTable = new MDTable<RawModuleRow>(Table.Module, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawTypeRefRow> TypeRefTable = new MDTable<RawTypeRefRow>(Table.TypeRef, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawTypeDefRow> TypeDefTable = new MDTable<RawTypeDefRow>(Table.TypeDef, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawFieldPtrRow> FieldPtrTable = new MDTable<RawFieldPtrRow>(Table.FieldPtr, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawFieldRow> FieldTable = new MDTable<RawFieldRow>(Table.Field, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawMethodPtrRow> MethodPtrTable = new MDTable<RawMethodPtrRow>(Table.MethodPtr, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawMethodRow> MethodTable = new MDTable<RawMethodRow>(Table.Method, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawParamPtrRow> ParamPtrTable = new MDTable<RawParamPtrRow>(Table.ParamPtr, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawParamRow> ParamTable = new MDTable<RawParamRow>(Table.Param, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawInterfaceImplRow> InterfaceImplTable = new MDTable<RawInterfaceImplRow>(Table.InterfaceImpl, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawMemberRefRow> MemberRefTable = new MDTable<RawMemberRefRow>(Table.MemberRef, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawConstantRow> ConstantTable = new MDTable<RawConstantRow>(Table.Constant, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawCustomAttributeRow> CustomAttributeTable = new MDTable<RawCustomAttributeRow>(Table.CustomAttribute, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawFieldMarshalRow> FieldMarshalTable = new MDTable<RawFieldMarshalRow>(Table.FieldMarshal, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawDeclSecurityRow> DeclSecurityTable = new MDTable<RawDeclSecurityRow>(Table.DeclSecurity, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawClassLayoutRow> ClassLayoutTable = new MDTable<RawClassLayoutRow>(Table.ClassLayout, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawFieldLayoutRow> FieldLayoutTable = new MDTable<RawFieldLayoutRow>(Table.FieldLayout, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawStandAloneSigRow> StandAloneSigTable = new MDTable<RawStandAloneSigRow>(Table.StandAloneSig, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawEventMapRow> EventMapTable = new MDTable<RawEventMapRow>(Table.EventMap, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawEventPtrRow> EventPtrTable = new MDTable<RawEventPtrRow>(Table.EventPtr, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawEventRow> EventTable = new MDTable<RawEventRow>(Table.Event, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawPropertyMapRow> PropertyMapTable = new MDTable<RawPropertyMapRow>(Table.PropertyMap, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawPropertyPtrRow> PropertyPtrTable = new MDTable<RawPropertyPtrRow>(Table.PropertyPtr, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawPropertyRow> PropertyTable = new MDTable<RawPropertyRow>(Table.Property, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawMethodSemanticsRow> MethodSemanticsTable = new MDTable<RawMethodSemanticsRow>(Table.MethodSemantics, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawMethodImplRow> MethodImplTable = new MDTable<RawMethodImplRow>(Table.MethodImpl, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawModuleRefRow> ModuleRefTable = new MDTable<RawModuleRefRow>(Table.ModuleRef, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawTypeSpecRow> TypeSpecTable = new MDTable<RawTypeSpecRow>(Table.TypeSpec, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawImplMapRow> ImplMapTable = new MDTable<RawImplMapRow>(Table.ImplMap, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawFieldRVARow> FieldRVATable = new MDTable<RawFieldRVARow>(Table.FieldRVA, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawENCLogRow> ENCLogTable = new MDTable<RawENCLogRow>(Table.ENCLog, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawENCMapRow> ENCMapTable = new MDTable<RawENCMapRow>(Table.ENCMap, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawAssemblyRow> AssemblyTable = new MDTable<RawAssemblyRow>(Table.Assembly, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawAssemblyProcessorRow> AssemblyProcessorTable = new MDTable<RawAssemblyProcessorRow>(Table.AssemblyProcessor, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawAssemblyOSRow> AssemblyOSTable = new MDTable<RawAssemblyOSRow>(Table.AssemblyOS, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawAssemblyRefRow> AssemblyRefTable = new MDTable<RawAssemblyRefRow>(Table.AssemblyRef, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawAssemblyRefProcessorRow> AssemblyRefProcessorTable = new MDTable<RawAssemblyRefProcessorRow>(Table.AssemblyRefProcessor, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawAssemblyRefOSRow> AssemblyRefOSTable = new MDTable<RawAssemblyRefOSRow>(Table.AssemblyRefOS, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawFileRow> FileTable = new MDTable<RawFileRow>(Table.File, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawExportedTypeRow> ExportedTypeTable = new MDTable<RawExportedTypeRow>(Table.ExportedType, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawManifestResourceRow> ManifestResourceTable = new MDTable<RawManifestResourceRow>(Table.ManifestResource, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawNestedClassRow> NestedClassTable = new MDTable<RawNestedClassRow>(Table.NestedClass, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawGenericParamRow> GenericParamTable = new MDTable<RawGenericParamRow>(Table.GenericParam, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawMethodSpecRow> MethodSpecTable = new MDTable<RawMethodSpecRow>(Table.MethodSpec, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawGenericParamConstraintRow> GenericParamConstraintTable = new MDTable<RawGenericParamConstraintRow>(Table.GenericParamConstraint, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawDocumentRow> DocumentTable = new MDTable<RawDocumentRow>(Table.Document, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawMethodDebugInformationRow> MethodDebugInformationTable = new MDTable<RawMethodDebugInformationRow>(Table.MethodDebugInformation, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawLocalScopeRow> LocalScopeTable = new MDTable<RawLocalScopeRow>(Table.LocalScope, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawLocalVariableRow> LocalVariableTable = new MDTable<RawLocalVariableRow>(Table.LocalVariable, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawLocalConstantRow> LocalConstantTable = new MDTable<RawLocalConstantRow>(Table.LocalConstant, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawImportScopeRow> ImportScopeTable = new MDTable<RawImportScopeRow>(Table.ImportScope, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawStateMachineMethodRow> StateMachineMethodTable = new MDTable<RawStateMachineMethodRow>(Table.StateMachineMethod, RawRowEqualityComparer.Instance);

	public readonly MDTable<RawCustomDebugInformationRow> CustomDebugInformationTable = new MDTable<RawCustomDebugInformationRow>(Table.CustomDebugInformation, RawRowEqualityComparer.Instance);

	public readonly IMDTable[] Tables;

	private uint[] systemTables;

	public FileOffset FileOffset => offset;

	public RVA RVA => rva;

	public string Name
	{
		get
		{
			if (!IsENC)
			{
				return "#~";
			}
			return "#-";
		}
	}

	public bool IsEmpty => false;

	public bool IsENC
	{
		get
		{
			if (options.UseENC.HasValue)
			{
				return options.UseENC.Value;
			}
			if (!hasDeletedRows && FieldPtrTable.IsEmpty && MethodPtrTable.IsEmpty && ParamPtrTable.IsEmpty && EventPtrTable.IsEmpty && PropertyPtrTable.IsEmpty && (InterfaceImplTable.IsEmpty || InterfaceImplTable.IsSorted) && (ConstantTable.IsEmpty || ConstantTable.IsSorted) && (CustomAttributeTable.IsEmpty || CustomAttributeTable.IsSorted) && (FieldMarshalTable.IsEmpty || FieldMarshalTable.IsSorted) && (DeclSecurityTable.IsEmpty || DeclSecurityTable.IsSorted) && (ClassLayoutTable.IsEmpty || ClassLayoutTable.IsSorted) && (FieldLayoutTable.IsEmpty || FieldLayoutTable.IsSorted) && (EventMapTable.IsEmpty || EventMapTable.IsSorted) && (PropertyMapTable.IsEmpty || PropertyMapTable.IsSorted) && (MethodSemanticsTable.IsEmpty || MethodSemanticsTable.IsSorted) && (MethodImplTable.IsEmpty || MethodImplTable.IsSorted) && (ImplMapTable.IsEmpty || ImplMapTable.IsSorted) && (FieldRVATable.IsEmpty || FieldRVATable.IsSorted) && (NestedClassTable.IsEmpty || NestedClassTable.IsSorted) && (GenericParamTable.IsEmpty || GenericParamTable.IsSorted))
			{
				if (!GenericParamConstraintTable.IsEmpty)
				{
					return !GenericParamConstraintTable.IsSorted;
				}
				return false;
			}
			return true;
		}
	}

	public bool HasDeletedRows
	{
		get
		{
			return hasDeletedRows;
		}
		set
		{
			hasDeletedRows = value;
		}
	}

	public bool BigStrings
	{
		get
		{
			return bigStrings;
		}
		set
		{
			bigStrings = value;
		}
	}

	public bool BigGuid
	{
		get
		{
			return bigGuid;
		}
		set
		{
			bigGuid = value;
		}
	}

	public bool BigBlob
	{
		get
		{
			return bigBlob;
		}
		set
		{
			bigBlob = value;
		}
	}

	public TablesHeap(Metadata metadata, TablesHeapOptions options)
	{
		this.metadata = metadata;
		this.options = options ?? new TablesHeapOptions();
		hasDeletedRows = this.options.HasDeletedRows.GetValueOrDefault();
		Tables = new IMDTable[56]
		{
			ModuleTable,
			TypeRefTable,
			TypeDefTable,
			FieldPtrTable,
			FieldTable,
			MethodPtrTable,
			MethodTable,
			ParamPtrTable,
			ParamTable,
			InterfaceImplTable,
			MemberRefTable,
			ConstantTable,
			CustomAttributeTable,
			FieldMarshalTable,
			DeclSecurityTable,
			ClassLayoutTable,
			FieldLayoutTable,
			StandAloneSigTable,
			EventMapTable,
			EventPtrTable,
			EventTable,
			PropertyMapTable,
			PropertyPtrTable,
			PropertyTable,
			MethodSemanticsTable,
			MethodImplTable,
			ModuleRefTable,
			TypeSpecTable,
			ImplMapTable,
			FieldRVATable,
			ENCLogTable,
			ENCMapTable,
			AssemblyTable,
			AssemblyProcessorTable,
			AssemblyOSTable,
			AssemblyRefTable,
			AssemblyRefProcessorTable,
			AssemblyRefOSTable,
			FileTable,
			ExportedTypeTable,
			ManifestResourceTable,
			NestedClassTable,
			GenericParamTable,
			MethodSpecTable,
			GenericParamConstraintTable,
			new MDTable<RawDummyRow>((Table)45, RawDummyRow.Comparer),
			new MDTable<RawDummyRow>((Table)46, RawDummyRow.Comparer),
			new MDTable<RawDummyRow>((Table)47, RawDummyRow.Comparer),
			DocumentTable,
			MethodDebugInformationTable,
			LocalScopeTable,
			LocalVariableTable,
			LocalConstantTable,
			ImportScopeTable,
			StateMachineMethodTable,
			CustomDebugInformationTable
		};
	}

	public void SetReadOnly()
	{
		IMDTable[] tables = Tables;
		for (int i = 0; i < tables.Length; i++)
		{
			tables[i].SetReadOnly();
		}
	}

	public void SetOffset(FileOffset offset, RVA rva)
	{
		this.offset = offset;
		this.rva = rva;
	}

	public uint GetFileLength()
	{
		if (length == 0)
		{
			CalculateLength();
		}
		return Utils.AlignUp(length, 4u);
	}

	public uint GetVirtualSize()
	{
		return GetFileLength();
	}

	public uint CalculateAlignment()
	{
		return 0u;
	}

	public void CalculateLength()
	{
		if (length != 0)
		{
			return;
		}
		SetReadOnly();
		majorVersion = options.MajorVersion ?? 2;
		minorVersion = options.MinorVersion.GetValueOrDefault();
		if (((majorVersion << 8) | minorVersion) <= 256 && (!GenericParamTable.IsEmpty || !MethodSpecTable.IsEmpty || !GenericParamConstraintTable.IsEmpty))
		{
			throw new ModuleWriterException("Tables heap version <= v1.0 but generic tables are not empty");
		}
		DotNetTableSizes dotNetTableSizes = new DotNetTableSizes();
		TableInfo[] array = dotNetTableSizes.CreateTables(majorVersion, minorVersion);
		uint[] rowCounts = GetRowCounts();
		uint[] array2 = rowCounts;
		if (systemTables != null)
		{
			array2 = new uint[rowCounts.Length];
			for (int i = 0; i < rowCounts.Length; i++)
			{
				if (DotNetTableSizes.IsSystemTable((Table)i))
				{
					array2[i] = systemTables[i];
				}
				else
				{
					array2[i] = rowCounts[i];
				}
			}
		}
		dotNetTableSizes.InitializeSizes(bigStrings, bigGuid, bigBlob, rowCounts, array2, options.ForceBigColumns.GetValueOrDefault());
		for (int j = 0; j < Tables.Length; j++)
		{
			Tables[j].TableInfo = array[j];
		}
		length = 24u;
		IMDTable[] tables = Tables;
		foreach (IMDTable iMDTable in tables)
		{
			if (!iMDTable.IsEmpty)
			{
				length += (uint)(4 + iMDTable.TableInfo.RowSize * iMDTable.Rows);
			}
		}
		if (options.ExtraData.HasValue)
		{
			length += 4u;
		}
	}

	private uint[] GetRowCounts()
	{
		uint[] array = new uint[Tables.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (uint)Tables[i].Rows;
		}
		return array;
	}

	internal void GetSystemTableRows(out ulong mask, uint[] tables)
	{
		if (tables.Length != 64)
		{
			throw new InvalidOperationException();
		}
		ulong validMask = GetValidMask();
		ulong num = 1uL;
		mask = 0uL;
		int num2 = 0;
		while (num2 < 64)
		{
			if (DotNetTableSizes.IsSystemTable((Table)num2))
			{
				if ((validMask & num) != 0L)
				{
					tables[num2] = (uint)Tables[num2].Rows;
					mask |= num;
				}
				else
				{
					tables[num2] = 0u;
				}
			}
			else
			{
				tables[num2] = 0u;
			}
			num2++;
			num <<= 1;
		}
	}

	internal void SetSystemTableRows(uint[] systemTables)
	{
		this.systemTables = (uint[])systemTables.Clone();
	}

	public void WriteTo(DataWriter writer)
	{
		writer.WriteUInt32(options.Reserved1.GetValueOrDefault());
		writer.WriteByte(majorVersion);
		writer.WriteByte(minorVersion);
		writer.WriteByte((byte)GetMDStreamFlags());
		writer.WriteByte(GetLog2Rid());
		writer.WriteUInt64(GetValidMask());
		writer.WriteUInt64(GetSortedMask());
		IMDTable[] tables = Tables;
		foreach (IMDTable iMDTable in tables)
		{
			if (!iMDTable.IsEmpty)
			{
				writer.WriteInt32(iMDTable.Rows);
			}
		}
		if (options.ExtraData.HasValue)
		{
			writer.WriteUInt32(options.ExtraData.Value);
		}
		writer.Write(metadata, ModuleTable);
		writer.Write(metadata, TypeRefTable);
		writer.Write(metadata, TypeDefTable);
		writer.Write(metadata, FieldPtrTable);
		writer.Write(metadata, FieldTable);
		writer.Write(metadata, MethodPtrTable);
		writer.Write(metadata, MethodTable);
		writer.Write(metadata, ParamPtrTable);
		writer.Write(metadata, ParamTable);
		writer.Write(metadata, InterfaceImplTable);
		writer.Write(metadata, MemberRefTable);
		writer.Write(metadata, ConstantTable);
		writer.Write(metadata, CustomAttributeTable);
		writer.Write(metadata, FieldMarshalTable);
		writer.Write(metadata, DeclSecurityTable);
		writer.Write(metadata, ClassLayoutTable);
		writer.Write(metadata, FieldLayoutTable);
		writer.Write(metadata, StandAloneSigTable);
		writer.Write(metadata, EventMapTable);
		writer.Write(metadata, EventPtrTable);
		writer.Write(metadata, EventTable);
		writer.Write(metadata, PropertyMapTable);
		writer.Write(metadata, PropertyPtrTable);
		writer.Write(metadata, PropertyTable);
		writer.Write(metadata, MethodSemanticsTable);
		writer.Write(metadata, MethodImplTable);
		writer.Write(metadata, ModuleRefTable);
		writer.Write(metadata, TypeSpecTable);
		writer.Write(metadata, ImplMapTable);
		writer.Write(metadata, FieldRVATable);
		writer.Write(metadata, ENCLogTable);
		writer.Write(metadata, ENCMapTable);
		writer.Write(metadata, AssemblyTable);
		writer.Write(metadata, AssemblyProcessorTable);
		writer.Write(metadata, AssemblyOSTable);
		writer.Write(metadata, AssemblyRefTable);
		writer.Write(metadata, AssemblyRefProcessorTable);
		writer.Write(metadata, AssemblyRefOSTable);
		writer.Write(metadata, FileTable);
		writer.Write(metadata, ExportedTypeTable);
		writer.Write(metadata, ManifestResourceTable);
		writer.Write(metadata, NestedClassTable);
		writer.Write(metadata, GenericParamTable);
		writer.Write(metadata, MethodSpecTable);
		writer.Write(metadata, GenericParamConstraintTable);
		writer.Write(metadata, DocumentTable);
		writer.Write(metadata, MethodDebugInformationTable);
		writer.Write(metadata, LocalScopeTable);
		writer.Write(metadata, LocalVariableTable);
		writer.Write(metadata, LocalConstantTable);
		writer.Write(metadata, ImportScopeTable);
		writer.Write(metadata, StateMachineMethodTable);
		writer.Write(metadata, CustomDebugInformationTable);
		writer.WriteZeroes((int)(Utils.AlignUp(length, 4u) - length));
	}

	private MDStreamFlags GetMDStreamFlags()
	{
		MDStreamFlags mDStreamFlags = (MDStreamFlags)0;
		if (bigStrings)
		{
			mDStreamFlags |= MDStreamFlags.BigStrings;
		}
		if (bigGuid)
		{
			mDStreamFlags |= MDStreamFlags.BigGUID;
		}
		if (bigBlob)
		{
			mDStreamFlags |= MDStreamFlags.BigBlob;
		}
		if (options.ExtraData.HasValue)
		{
			mDStreamFlags |= MDStreamFlags.ExtraData;
		}
		if (hasDeletedRows)
		{
			mDStreamFlags |= MDStreamFlags.HasDelete;
		}
		return mDStreamFlags;
	}

	private byte GetLog2Rid()
	{
		return options.Log2Rid ?? 1;
	}

	private ulong GetValidMask()
	{
		ulong num = 0uL;
		IMDTable[] tables = Tables;
		foreach (IMDTable iMDTable in tables)
		{
			if (!iMDTable.IsEmpty)
			{
				num |= (ulong)(1L << (int)iMDTable.Table);
			}
		}
		return num;
	}

	private ulong GetSortedMask()
	{
		ulong num = 0uL;
		IMDTable[] tables = Tables;
		foreach (IMDTable iMDTable in tables)
		{
			if (iMDTable.IsSorted)
			{
				num |= (ulong)(1L << (int)iMDTable.Table);
			}
		}
		return num;
	}

	public override string ToString()
	{
		return Name;
	}
}
