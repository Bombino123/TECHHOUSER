using System;
using System.Runtime.InteropServices;
using dnlib.IO;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public sealed class TablesStream : DotNetStream
{
	private bool initialized;

	private uint reserved1;

	private byte majorVersion;

	private byte minorVersion;

	private MDStreamFlags flags;

	private byte log2Rid;

	private ulong validMask;

	private ulong sortedMask;

	private uint extraData;

	private MDTable[] mdTables;

	private uint mdTablesPos;

	private IColumnReader columnReader;

	private IRowReader<RawMethodRow> methodRowReader;

	private readonly CLRRuntimeReaderKind runtime;

	public MDTable ModuleTable { get; private set; }

	public MDTable TypeRefTable { get; private set; }

	public MDTable TypeDefTable { get; private set; }

	public MDTable FieldPtrTable { get; private set; }

	public MDTable FieldTable { get; private set; }

	public MDTable MethodPtrTable { get; private set; }

	public MDTable MethodTable { get; private set; }

	public MDTable ParamPtrTable { get; private set; }

	public MDTable ParamTable { get; private set; }

	public MDTable InterfaceImplTable { get; private set; }

	public MDTable MemberRefTable { get; private set; }

	public MDTable ConstantTable { get; private set; }

	public MDTable CustomAttributeTable { get; private set; }

	public MDTable FieldMarshalTable { get; private set; }

	public MDTable DeclSecurityTable { get; private set; }

	public MDTable ClassLayoutTable { get; private set; }

	public MDTable FieldLayoutTable { get; private set; }

	public MDTable StandAloneSigTable { get; private set; }

	public MDTable EventMapTable { get; private set; }

	public MDTable EventPtrTable { get; private set; }

	public MDTable EventTable { get; private set; }

	public MDTable PropertyMapTable { get; private set; }

	public MDTable PropertyPtrTable { get; private set; }

	public MDTable PropertyTable { get; private set; }

	public MDTable MethodSemanticsTable { get; private set; }

	public MDTable MethodImplTable { get; private set; }

	public MDTable ModuleRefTable { get; private set; }

	public MDTable TypeSpecTable { get; private set; }

	public MDTable ImplMapTable { get; private set; }

	public MDTable FieldRVATable { get; private set; }

	public MDTable ENCLogTable { get; private set; }

	public MDTable ENCMapTable { get; private set; }

	public MDTable AssemblyTable { get; private set; }

	public MDTable AssemblyProcessorTable { get; private set; }

	public MDTable AssemblyOSTable { get; private set; }

	public MDTable AssemblyRefTable { get; private set; }

	public MDTable AssemblyRefProcessorTable { get; private set; }

	public MDTable AssemblyRefOSTable { get; private set; }

	public MDTable FileTable { get; private set; }

	public MDTable ExportedTypeTable { get; private set; }

	public MDTable ManifestResourceTable { get; private set; }

	public MDTable NestedClassTable { get; private set; }

	public MDTable GenericParamTable { get; private set; }

	public MDTable MethodSpecTable { get; private set; }

	public MDTable GenericParamConstraintTable { get; private set; }

	public MDTable DocumentTable { get; private set; }

	public MDTable MethodDebugInformationTable { get; private set; }

	public MDTable LocalScopeTable { get; private set; }

	public MDTable LocalVariableTable { get; private set; }

	public MDTable LocalConstantTable { get; private set; }

	public MDTable ImportScopeTable { get; private set; }

	public MDTable StateMachineMethodTable { get; private set; }

	public MDTable CustomDebugInformationTable { get; private set; }

	public IColumnReader ColumnReader
	{
		get
		{
			return columnReader;
		}
		set
		{
			columnReader = value;
		}
	}

	public IRowReader<RawMethodRow> MethodRowReader
	{
		get
		{
			return methodRowReader;
		}
		set
		{
			methodRowReader = value;
		}
	}

	public uint Reserved1 => reserved1;

	public ushort Version => (ushort)((majorVersion << 8) | minorVersion);

	public MDStreamFlags Flags => flags;

	public byte Log2Rid => log2Rid;

	public ulong ValidMask => validMask;

	public ulong SortedMask => sortedMask;

	public uint ExtraData => extraData;

	public MDTable[] MDTables => mdTables;

	public bool HasBigStrings => (flags & MDStreamFlags.BigStrings) != 0;

	public bool HasBigGUID => (flags & MDStreamFlags.BigGUID) != 0;

	public bool HasBigBlob => (flags & MDStreamFlags.BigBlob) != 0;

	public bool HasPadding
	{
		get
		{
			if (runtime == CLRRuntimeReaderKind.CLR)
			{
				return (flags & MDStreamFlags.Padding) != 0;
			}
			return false;
		}
	}

	public bool HasDeltaOnly
	{
		get
		{
			if (runtime == CLRRuntimeReaderKind.CLR)
			{
				return (flags & MDStreamFlags.DeltaOnly) != 0;
			}
			return false;
		}
	}

	public bool HasExtraData
	{
		get
		{
			if (runtime == CLRRuntimeReaderKind.CLR)
			{
				return (flags & MDStreamFlags.ExtraData) != 0;
			}
			return false;
		}
	}

	public bool HasDelete
	{
		get
		{
			if (runtime == CLRRuntimeReaderKind.CLR)
			{
				return (flags & MDStreamFlags.HasDelete) != 0;
			}
			return false;
		}
	}

	public TablesStream(DataReaderFactory mdReaderFactory, uint metadataBaseOffset, StreamHeader streamHeader)
		: this(mdReaderFactory, metadataBaseOffset, streamHeader, CLRRuntimeReaderKind.CLR)
	{
	}

	public TablesStream(DataReaderFactory mdReaderFactory, uint metadataBaseOffset, StreamHeader streamHeader, CLRRuntimeReaderKind runtime)
		: base(mdReaderFactory, metadataBaseOffset, streamHeader)
	{
		this.runtime = runtime;
	}

	public void Initialize(uint[] typeSystemTableRows)
	{
		Initialize(typeSystemTableRows, forceAllBig: false);
	}

	internal void Initialize(uint[] typeSystemTableRows, bool forceAllBig)
	{
		if (initialized)
		{
			throw new Exception("Initialize() has already been called");
		}
		initialized = true;
		DataReader dataReader = base.dataReader;
		reserved1 = dataReader.ReadUInt32();
		majorVersion = dataReader.ReadByte();
		minorVersion = dataReader.ReadByte();
		flags = (MDStreamFlags)dataReader.ReadByte();
		log2Rid = dataReader.ReadByte();
		validMask = dataReader.ReadUInt64();
		sortedMask = dataReader.ReadUInt64();
		if (runtime == CLRRuntimeReaderKind.Mono)
		{
			sortedMask = ulong.MaxValue;
		}
		DotNetTableSizes dotNetTableSizes = new DotNetTableSizes();
		byte b = majorVersion;
		byte b2 = minorVersion;
		if (runtime == CLRRuntimeReaderKind.Mono)
		{
			b = 2;
			b2 = 0;
		}
		int maxPresentTables;
		TableInfo[] array = dotNetTableSizes.CreateTables(b, b2, out maxPresentTables);
		if (typeSystemTableRows != null)
		{
			maxPresentTables = 56;
		}
		mdTables = new MDTable[array.Length];
		ulong num = validMask;
		uint[] array2 = new uint[64];
		for (int i = 0; i < 64; i++)
		{
			uint num2 = (((num & 1) != 0L) ? dataReader.ReadUInt32() : 0u);
			num2 &= 0xFFFFFFu;
			if (i >= maxPresentTables)
			{
				num2 = 0u;
			}
			array2[i] = num2;
			if (i < mdTables.Length)
			{
				mdTables[i] = new MDTable((Table)i, num2, array[i]);
			}
			num >>= 1;
		}
		if (HasExtraData)
		{
			extraData = dataReader.ReadUInt32();
		}
		uint[] array3 = array2;
		if (typeSystemTableRows != null)
		{
			array3 = new uint[array2.Length];
			for (int j = 0; j < 64; j++)
			{
				if (DotNetTableSizes.IsSystemTable((Table)j))
				{
					array3[j] = typeSystemTableRows[j];
				}
				else
				{
					array3[j] = array2[j];
				}
			}
		}
		dotNetTableSizes.InitializeSizes(HasBigStrings, HasBigGUID, HasBigBlob, array2, array3, forceAllBig);
		mdTablesPos = dataReader.Position;
		InitializeMdTableReaders();
		InitializeTables();
	}

	protected override void OnReaderRecreated()
	{
		InitializeMdTableReaders();
	}

	private void InitializeMdTableReaders()
	{
		DataReader dataReader = base.dataReader;
		dataReader.Position = mdTablesPos;
		uint num = dataReader.Position;
		MDTable[] array = mdTables;
		foreach (MDTable mDTable in array)
		{
			uint num2 = (uint)mDTable.TableInfo.RowSize * mDTable.Rows;
			if (num > dataReader.Length)
			{
				num = dataReader.Length;
			}
			if ((ulong)((long)num + (long)num2) > (ulong)dataReader.Length)
			{
				num2 = dataReader.Length - num;
			}
			mDTable.DataReader = dataReader.Slice(num, num2);
			uint num3 = num + num2;
			if (num3 < num)
			{
				throw new BadImageFormatException("Too big MD table");
			}
			num = num3;
		}
	}

	private void InitializeTables()
	{
		ModuleTable = mdTables[0];
		TypeRefTable = mdTables[1];
		TypeDefTable = mdTables[2];
		FieldPtrTable = mdTables[3];
		FieldTable = mdTables[4];
		MethodPtrTable = mdTables[5];
		MethodTable = mdTables[6];
		ParamPtrTable = mdTables[7];
		ParamTable = mdTables[8];
		InterfaceImplTable = mdTables[9];
		MemberRefTable = mdTables[10];
		ConstantTable = mdTables[11];
		CustomAttributeTable = mdTables[12];
		FieldMarshalTable = mdTables[13];
		DeclSecurityTable = mdTables[14];
		ClassLayoutTable = mdTables[15];
		FieldLayoutTable = mdTables[16];
		StandAloneSigTable = mdTables[17];
		EventMapTable = mdTables[18];
		EventPtrTable = mdTables[19];
		EventTable = mdTables[20];
		PropertyMapTable = mdTables[21];
		PropertyPtrTable = mdTables[22];
		PropertyTable = mdTables[23];
		MethodSemanticsTable = mdTables[24];
		MethodImplTable = mdTables[25];
		ModuleRefTable = mdTables[26];
		TypeSpecTable = mdTables[27];
		ImplMapTable = mdTables[28];
		FieldRVATable = mdTables[29];
		ENCLogTable = mdTables[30];
		ENCMapTable = mdTables[31];
		AssemblyTable = mdTables[32];
		AssemblyProcessorTable = mdTables[33];
		AssemblyOSTable = mdTables[34];
		AssemblyRefTable = mdTables[35];
		AssemblyRefProcessorTable = mdTables[36];
		AssemblyRefOSTable = mdTables[37];
		FileTable = mdTables[38];
		ExportedTypeTable = mdTables[39];
		ManifestResourceTable = mdTables[40];
		NestedClassTable = mdTables[41];
		GenericParamTable = mdTables[42];
		MethodSpecTable = mdTables[43];
		GenericParamConstraintTable = mdTables[44];
		DocumentTable = mdTables[48];
		MethodDebugInformationTable = mdTables[49];
		LocalScopeTable = mdTables[50];
		LocalVariableTable = mdTables[51];
		LocalConstantTable = mdTables[52];
		ImportScopeTable = mdTables[53];
		StateMachineMethodTable = mdTables[54];
		CustomDebugInformationTable = mdTables[55];
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			MDTable[] array = mdTables;
			if (array != null)
			{
				MDTable[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i]?.Dispose();
				}
				mdTables = null;
			}
		}
		base.Dispose(disposing);
	}

	public MDTable Get(Table table)
	{
		if ((uint)table >= (uint)mdTables.Length)
		{
			return null;
		}
		return mdTables[(uint)table];
	}

	public bool HasTable(Table table)
	{
		return (uint)table < (uint)mdTables.Length;
	}

	public bool IsSorted(MDTable table)
	{
		int table2 = (int)table.Table;
		if ((uint)table2 >= 64u)
		{
			return false;
		}
		return (sortedMask & (ulong)(1L << table2)) != 0;
	}

	public bool TryReadModuleRow(uint rid, out RawModuleRow row)
	{
		MDTable moduleTable = ModuleTable;
		if (moduleTable.IsInvalidRID(rid))
		{
			row = default(RawModuleRow);
			return false;
		}
		DataReader reader = moduleTable.DataReader;
		reader.Position = (rid - 1) * (uint)moduleTable.TableInfo.RowSize;
		row = new RawModuleRow(reader.Unsafe_ReadUInt16(), moduleTable.Column1.Unsafe_Read24(ref reader), moduleTable.Column2.Unsafe_Read24(ref reader), moduleTable.Column3.Unsafe_Read24(ref reader), moduleTable.Column4.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadTypeRefRow(uint rid, out RawTypeRefRow row)
	{
		MDTable typeRefTable = TypeRefTable;
		if (typeRefTable.IsInvalidRID(rid))
		{
			row = default(RawTypeRefRow);
			return false;
		}
		DataReader reader = typeRefTable.DataReader;
		reader.Position = (rid - 1) * (uint)typeRefTable.TableInfo.RowSize;
		row = new RawTypeRefRow(typeRefTable.Column0.Unsafe_Read24(ref reader), typeRefTable.Column1.Unsafe_Read24(ref reader), typeRefTable.Column2.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadTypeDefRow(uint rid, out RawTypeDefRow row)
	{
		MDTable typeDefTable = TypeDefTable;
		if (typeDefTable.IsInvalidRID(rid))
		{
			row = default(RawTypeDefRow);
			return false;
		}
		DataReader reader = typeDefTable.DataReader;
		reader.Position = (rid - 1) * (uint)typeDefTable.TableInfo.RowSize;
		row = new RawTypeDefRow(reader.Unsafe_ReadUInt32(), typeDefTable.Column1.Unsafe_Read24(ref reader), typeDefTable.Column2.Unsafe_Read24(ref reader), typeDefTable.Column3.Unsafe_Read24(ref reader), typeDefTable.Column4.Unsafe_Read24(ref reader), typeDefTable.Column5.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadFieldPtrRow(uint rid, out RawFieldPtrRow row)
	{
		MDTable fieldPtrTable = FieldPtrTable;
		if (fieldPtrTable.IsInvalidRID(rid))
		{
			row = default(RawFieldPtrRow);
			return false;
		}
		DataReader reader = fieldPtrTable.DataReader;
		reader.Position = (rid - 1) * (uint)fieldPtrTable.TableInfo.RowSize;
		row = new RawFieldPtrRow(fieldPtrTable.Column0.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadFieldRow(uint rid, out RawFieldRow row)
	{
		MDTable fieldTable = FieldTable;
		if (fieldTable.IsInvalidRID(rid))
		{
			row = default(RawFieldRow);
			return false;
		}
		DataReader reader = fieldTable.DataReader;
		reader.Position = (rid - 1) * (uint)fieldTable.TableInfo.RowSize;
		row = new RawFieldRow(reader.Unsafe_ReadUInt16(), fieldTable.Column1.Unsafe_Read24(ref reader), fieldTable.Column2.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadMethodPtrRow(uint rid, out RawMethodPtrRow row)
	{
		MDTable methodPtrTable = MethodPtrTable;
		if (methodPtrTable.IsInvalidRID(rid))
		{
			row = default(RawMethodPtrRow);
			return false;
		}
		DataReader reader = methodPtrTable.DataReader;
		reader.Position = (rid - 1) * (uint)methodPtrTable.TableInfo.RowSize;
		row = new RawMethodPtrRow(methodPtrTable.Column0.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadMethodRow(uint rid, out RawMethodRow row)
	{
		MDTable methodTable = MethodTable;
		if (methodTable.IsInvalidRID(rid))
		{
			row = default(RawMethodRow);
			return false;
		}
		IRowReader<RawMethodRow> rowReader = methodRowReader;
		if (rowReader != null && rowReader.TryReadRow(rid, out row))
		{
			return true;
		}
		DataReader reader = methodTable.DataReader;
		reader.Position = (rid - 1) * (uint)methodTable.TableInfo.RowSize;
		row = new RawMethodRow(reader.Unsafe_ReadUInt32(), reader.Unsafe_ReadUInt16(), reader.Unsafe_ReadUInt16(), methodTable.Column3.Unsafe_Read24(ref reader), methodTable.Column4.Unsafe_Read24(ref reader), methodTable.Column5.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadParamPtrRow(uint rid, out RawParamPtrRow row)
	{
		MDTable paramPtrTable = ParamPtrTable;
		if (paramPtrTable.IsInvalidRID(rid))
		{
			row = default(RawParamPtrRow);
			return false;
		}
		DataReader reader = paramPtrTable.DataReader;
		reader.Position = (rid - 1) * (uint)paramPtrTable.TableInfo.RowSize;
		row = new RawParamPtrRow(paramPtrTable.Column0.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadParamRow(uint rid, out RawParamRow row)
	{
		MDTable paramTable = ParamTable;
		if (paramTable.IsInvalidRID(rid))
		{
			row = default(RawParamRow);
			return false;
		}
		DataReader reader = paramTable.DataReader;
		reader.Position = (rid - 1) * (uint)paramTable.TableInfo.RowSize;
		row = new RawParamRow(reader.Unsafe_ReadUInt16(), reader.Unsafe_ReadUInt16(), paramTable.Column2.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadInterfaceImplRow(uint rid, out RawInterfaceImplRow row)
	{
		MDTable interfaceImplTable = InterfaceImplTable;
		if (interfaceImplTable.IsInvalidRID(rid))
		{
			row = default(RawInterfaceImplRow);
			return false;
		}
		DataReader reader = interfaceImplTable.DataReader;
		reader.Position = (rid - 1) * (uint)interfaceImplTable.TableInfo.RowSize;
		row = new RawInterfaceImplRow(interfaceImplTable.Column0.Unsafe_Read24(ref reader), interfaceImplTable.Column1.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadMemberRefRow(uint rid, out RawMemberRefRow row)
	{
		MDTable memberRefTable = MemberRefTable;
		if (memberRefTable.IsInvalidRID(rid))
		{
			row = default(RawMemberRefRow);
			return false;
		}
		DataReader reader = memberRefTable.DataReader;
		reader.Position = (rid - 1) * (uint)memberRefTable.TableInfo.RowSize;
		row = new RawMemberRefRow(memberRefTable.Column0.Unsafe_Read24(ref reader), memberRefTable.Column1.Unsafe_Read24(ref reader), memberRefTable.Column2.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadConstantRow(uint rid, out RawConstantRow row)
	{
		MDTable constantTable = ConstantTable;
		if (constantTable.IsInvalidRID(rid))
		{
			row = default(RawConstantRow);
			return false;
		}
		DataReader reader = constantTable.DataReader;
		reader.Position = (rid - 1) * (uint)constantTable.TableInfo.RowSize;
		row = new RawConstantRow(reader.Unsafe_ReadByte(), reader.Unsafe_ReadByte(), constantTable.Column2.Unsafe_Read24(ref reader), constantTable.Column3.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadCustomAttributeRow(uint rid, out RawCustomAttributeRow row)
	{
		MDTable customAttributeTable = CustomAttributeTable;
		if (customAttributeTable.IsInvalidRID(rid))
		{
			row = default(RawCustomAttributeRow);
			return false;
		}
		DataReader reader = customAttributeTable.DataReader;
		reader.Position = (rid - 1) * (uint)customAttributeTable.TableInfo.RowSize;
		row = new RawCustomAttributeRow(customAttributeTable.Column0.Unsafe_Read24(ref reader), customAttributeTable.Column1.Unsafe_Read24(ref reader), customAttributeTable.Column2.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadFieldMarshalRow(uint rid, out RawFieldMarshalRow row)
	{
		MDTable fieldMarshalTable = FieldMarshalTable;
		if (fieldMarshalTable.IsInvalidRID(rid))
		{
			row = default(RawFieldMarshalRow);
			return false;
		}
		DataReader reader = fieldMarshalTable.DataReader;
		reader.Position = (rid - 1) * (uint)fieldMarshalTable.TableInfo.RowSize;
		row = new RawFieldMarshalRow(fieldMarshalTable.Column0.Unsafe_Read24(ref reader), fieldMarshalTable.Column1.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadDeclSecurityRow(uint rid, out RawDeclSecurityRow row)
	{
		MDTable declSecurityTable = DeclSecurityTable;
		if (declSecurityTable.IsInvalidRID(rid))
		{
			row = default(RawDeclSecurityRow);
			return false;
		}
		DataReader reader = declSecurityTable.DataReader;
		reader.Position = (rid - 1) * (uint)declSecurityTable.TableInfo.RowSize;
		row = new RawDeclSecurityRow((short)reader.Unsafe_ReadUInt16(), declSecurityTable.Column1.Unsafe_Read24(ref reader), declSecurityTable.Column2.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadClassLayoutRow(uint rid, out RawClassLayoutRow row)
	{
		MDTable classLayoutTable = ClassLayoutTable;
		if (classLayoutTable.IsInvalidRID(rid))
		{
			row = default(RawClassLayoutRow);
			return false;
		}
		DataReader reader = classLayoutTable.DataReader;
		reader.Position = (rid - 1) * (uint)classLayoutTable.TableInfo.RowSize;
		row = new RawClassLayoutRow(reader.Unsafe_ReadUInt16(), reader.Unsafe_ReadUInt32(), classLayoutTable.Column2.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadFieldLayoutRow(uint rid, out RawFieldLayoutRow row)
	{
		MDTable fieldLayoutTable = FieldLayoutTable;
		if (fieldLayoutTable.IsInvalidRID(rid))
		{
			row = default(RawFieldLayoutRow);
			return false;
		}
		DataReader reader = fieldLayoutTable.DataReader;
		reader.Position = (rid - 1) * (uint)fieldLayoutTable.TableInfo.RowSize;
		row = new RawFieldLayoutRow(reader.Unsafe_ReadUInt32(), fieldLayoutTable.Column1.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadStandAloneSigRow(uint rid, out RawStandAloneSigRow row)
	{
		MDTable standAloneSigTable = StandAloneSigTable;
		if (standAloneSigTable.IsInvalidRID(rid))
		{
			row = default(RawStandAloneSigRow);
			return false;
		}
		DataReader reader = standAloneSigTable.DataReader;
		reader.Position = (rid - 1) * (uint)standAloneSigTable.TableInfo.RowSize;
		row = new RawStandAloneSigRow(standAloneSigTable.Column0.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadEventMapRow(uint rid, out RawEventMapRow row)
	{
		MDTable eventMapTable = EventMapTable;
		if (eventMapTable.IsInvalidRID(rid))
		{
			row = default(RawEventMapRow);
			return false;
		}
		DataReader reader = eventMapTable.DataReader;
		reader.Position = (rid - 1) * (uint)eventMapTable.TableInfo.RowSize;
		row = new RawEventMapRow(eventMapTable.Column0.Unsafe_Read24(ref reader), eventMapTable.Column1.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadEventPtrRow(uint rid, out RawEventPtrRow row)
	{
		MDTable eventPtrTable = EventPtrTable;
		if (eventPtrTable.IsInvalidRID(rid))
		{
			row = default(RawEventPtrRow);
			return false;
		}
		DataReader reader = eventPtrTable.DataReader;
		reader.Position = (rid - 1) * (uint)eventPtrTable.TableInfo.RowSize;
		row = new RawEventPtrRow(eventPtrTable.Column0.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadEventRow(uint rid, out RawEventRow row)
	{
		MDTable eventTable = EventTable;
		if (eventTable.IsInvalidRID(rid))
		{
			row = default(RawEventRow);
			return false;
		}
		DataReader reader = eventTable.DataReader;
		reader.Position = (rid - 1) * (uint)eventTable.TableInfo.RowSize;
		row = new RawEventRow(reader.Unsafe_ReadUInt16(), eventTable.Column1.Unsafe_Read24(ref reader), eventTable.Column2.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadPropertyMapRow(uint rid, out RawPropertyMapRow row)
	{
		MDTable propertyMapTable = PropertyMapTable;
		if (propertyMapTable.IsInvalidRID(rid))
		{
			row = default(RawPropertyMapRow);
			return false;
		}
		DataReader reader = propertyMapTable.DataReader;
		reader.Position = (rid - 1) * (uint)propertyMapTable.TableInfo.RowSize;
		row = new RawPropertyMapRow(propertyMapTable.Column0.Unsafe_Read24(ref reader), propertyMapTable.Column1.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadPropertyPtrRow(uint rid, out RawPropertyPtrRow row)
	{
		MDTable propertyPtrTable = PropertyPtrTable;
		if (propertyPtrTable.IsInvalidRID(rid))
		{
			row = default(RawPropertyPtrRow);
			return false;
		}
		DataReader reader = propertyPtrTable.DataReader;
		reader.Position = (rid - 1) * (uint)propertyPtrTable.TableInfo.RowSize;
		row = new RawPropertyPtrRow(propertyPtrTable.Column0.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadPropertyRow(uint rid, out RawPropertyRow row)
	{
		MDTable propertyTable = PropertyTable;
		if (propertyTable.IsInvalidRID(rid))
		{
			row = default(RawPropertyRow);
			return false;
		}
		DataReader reader = propertyTable.DataReader;
		reader.Position = (rid - 1) * (uint)propertyTable.TableInfo.RowSize;
		row = new RawPropertyRow(reader.Unsafe_ReadUInt16(), propertyTable.Column1.Unsafe_Read24(ref reader), propertyTable.Column2.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadMethodSemanticsRow(uint rid, out RawMethodSemanticsRow row)
	{
		MDTable methodSemanticsTable = MethodSemanticsTable;
		if (methodSemanticsTable.IsInvalidRID(rid))
		{
			row = default(RawMethodSemanticsRow);
			return false;
		}
		DataReader reader = methodSemanticsTable.DataReader;
		reader.Position = (rid - 1) * (uint)methodSemanticsTable.TableInfo.RowSize;
		row = new RawMethodSemanticsRow(reader.Unsafe_ReadUInt16(), methodSemanticsTable.Column1.Unsafe_Read24(ref reader), methodSemanticsTable.Column2.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadMethodImplRow(uint rid, out RawMethodImplRow row)
	{
		MDTable methodImplTable = MethodImplTable;
		if (methodImplTable.IsInvalidRID(rid))
		{
			row = default(RawMethodImplRow);
			return false;
		}
		DataReader reader = methodImplTable.DataReader;
		reader.Position = (rid - 1) * (uint)methodImplTable.TableInfo.RowSize;
		row = new RawMethodImplRow(methodImplTable.Column0.Unsafe_Read24(ref reader), methodImplTable.Column1.Unsafe_Read24(ref reader), methodImplTable.Column2.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadModuleRefRow(uint rid, out RawModuleRefRow row)
	{
		MDTable moduleRefTable = ModuleRefTable;
		if (moduleRefTable.IsInvalidRID(rid))
		{
			row = default(RawModuleRefRow);
			return false;
		}
		DataReader reader = moduleRefTable.DataReader;
		reader.Position = (rid - 1) * (uint)moduleRefTable.TableInfo.RowSize;
		row = new RawModuleRefRow(moduleRefTable.Column0.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadTypeSpecRow(uint rid, out RawTypeSpecRow row)
	{
		MDTable typeSpecTable = TypeSpecTable;
		if (typeSpecTable.IsInvalidRID(rid))
		{
			row = default(RawTypeSpecRow);
			return false;
		}
		DataReader reader = typeSpecTable.DataReader;
		reader.Position = (rid - 1) * (uint)typeSpecTable.TableInfo.RowSize;
		row = new RawTypeSpecRow(typeSpecTable.Column0.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadImplMapRow(uint rid, out RawImplMapRow row)
	{
		MDTable implMapTable = ImplMapTable;
		if (implMapTable.IsInvalidRID(rid))
		{
			row = default(RawImplMapRow);
			return false;
		}
		DataReader reader = implMapTable.DataReader;
		reader.Position = (rid - 1) * (uint)implMapTable.TableInfo.RowSize;
		row = new RawImplMapRow(reader.Unsafe_ReadUInt16(), implMapTable.Column1.Unsafe_Read24(ref reader), implMapTable.Column2.Unsafe_Read24(ref reader), implMapTable.Column3.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadFieldRVARow(uint rid, out RawFieldRVARow row)
	{
		MDTable fieldRVATable = FieldRVATable;
		if (fieldRVATable.IsInvalidRID(rid))
		{
			row = default(RawFieldRVARow);
			return false;
		}
		DataReader reader = fieldRVATable.DataReader;
		reader.Position = (rid - 1) * (uint)fieldRVATable.TableInfo.RowSize;
		row = new RawFieldRVARow(reader.Unsafe_ReadUInt32(), fieldRVATable.Column1.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadENCLogRow(uint rid, out RawENCLogRow row)
	{
		MDTable eNCLogTable = ENCLogTable;
		if (eNCLogTable.IsInvalidRID(rid))
		{
			row = default(RawENCLogRow);
			return false;
		}
		DataReader dataReader = eNCLogTable.DataReader;
		dataReader.Position = (rid - 1) * (uint)eNCLogTable.TableInfo.RowSize;
		row = new RawENCLogRow(dataReader.Unsafe_ReadUInt32(), dataReader.Unsafe_ReadUInt32());
		return true;
	}

	public bool TryReadENCMapRow(uint rid, out RawENCMapRow row)
	{
		MDTable eNCMapTable = ENCMapTable;
		if (eNCMapTable.IsInvalidRID(rid))
		{
			row = default(RawENCMapRow);
			return false;
		}
		DataReader dataReader = eNCMapTable.DataReader;
		dataReader.Position = (rid - 1) * (uint)eNCMapTable.TableInfo.RowSize;
		row = new RawENCMapRow(dataReader.Unsafe_ReadUInt32());
		return true;
	}

	public bool TryReadAssemblyRow(uint rid, out RawAssemblyRow row)
	{
		MDTable assemblyTable = AssemblyTable;
		if (assemblyTable.IsInvalidRID(rid))
		{
			row = default(RawAssemblyRow);
			return false;
		}
		DataReader reader = assemblyTable.DataReader;
		reader.Position = (rid - 1) * (uint)assemblyTable.TableInfo.RowSize;
		row = new RawAssemblyRow(reader.Unsafe_ReadUInt32(), reader.Unsafe_ReadUInt16(), reader.Unsafe_ReadUInt16(), reader.Unsafe_ReadUInt16(), reader.Unsafe_ReadUInt16(), reader.Unsafe_ReadUInt32(), assemblyTable.Column6.Unsafe_Read24(ref reader), assemblyTable.Column7.Unsafe_Read24(ref reader), assemblyTable.Column8.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadAssemblyProcessorRow(uint rid, out RawAssemblyProcessorRow row)
	{
		MDTable assemblyProcessorTable = AssemblyProcessorTable;
		if (assemblyProcessorTable.IsInvalidRID(rid))
		{
			row = default(RawAssemblyProcessorRow);
			return false;
		}
		DataReader dataReader = assemblyProcessorTable.DataReader;
		dataReader.Position = (rid - 1) * (uint)assemblyProcessorTable.TableInfo.RowSize;
		row = new RawAssemblyProcessorRow(dataReader.Unsafe_ReadUInt32());
		return true;
	}

	public bool TryReadAssemblyOSRow(uint rid, out RawAssemblyOSRow row)
	{
		MDTable assemblyOSTable = AssemblyOSTable;
		if (assemblyOSTable.IsInvalidRID(rid))
		{
			row = default(RawAssemblyOSRow);
			return false;
		}
		DataReader dataReader = assemblyOSTable.DataReader;
		dataReader.Position = (rid - 1) * (uint)assemblyOSTable.TableInfo.RowSize;
		row = new RawAssemblyOSRow(dataReader.Unsafe_ReadUInt32(), dataReader.Unsafe_ReadUInt32(), dataReader.Unsafe_ReadUInt32());
		return true;
	}

	public bool TryReadAssemblyRefRow(uint rid, out RawAssemblyRefRow row)
	{
		MDTable assemblyRefTable = AssemblyRefTable;
		if (assemblyRefTable.IsInvalidRID(rid))
		{
			row = default(RawAssemblyRefRow);
			return false;
		}
		DataReader reader = assemblyRefTable.DataReader;
		reader.Position = (rid - 1) * (uint)assemblyRefTable.TableInfo.RowSize;
		row = new RawAssemblyRefRow(reader.Unsafe_ReadUInt16(), reader.Unsafe_ReadUInt16(), reader.Unsafe_ReadUInt16(), reader.Unsafe_ReadUInt16(), reader.Unsafe_ReadUInt32(), assemblyRefTable.Column5.Unsafe_Read24(ref reader), assemblyRefTable.Column6.Unsafe_Read24(ref reader), assemblyRefTable.Column7.Unsafe_Read24(ref reader), assemblyRefTable.Column8.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadAssemblyRefProcessorRow(uint rid, out RawAssemblyRefProcessorRow row)
	{
		MDTable assemblyRefProcessorTable = AssemblyRefProcessorTable;
		if (assemblyRefProcessorTable.IsInvalidRID(rid))
		{
			row = default(RawAssemblyRefProcessorRow);
			return false;
		}
		DataReader reader = assemblyRefProcessorTable.DataReader;
		reader.Position = (rid - 1) * (uint)assemblyRefProcessorTable.TableInfo.RowSize;
		row = new RawAssemblyRefProcessorRow(reader.Unsafe_ReadUInt32(), assemblyRefProcessorTable.Column1.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadAssemblyRefOSRow(uint rid, out RawAssemblyRefOSRow row)
	{
		MDTable assemblyRefOSTable = AssemblyRefOSTable;
		if (assemblyRefOSTable.IsInvalidRID(rid))
		{
			row = default(RawAssemblyRefOSRow);
			return false;
		}
		DataReader reader = assemblyRefOSTable.DataReader;
		reader.Position = (rid - 1) * (uint)assemblyRefOSTable.TableInfo.RowSize;
		row = new RawAssemblyRefOSRow(reader.Unsafe_ReadUInt32(), reader.Unsafe_ReadUInt32(), reader.Unsafe_ReadUInt32(), assemblyRefOSTable.Column3.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadFileRow(uint rid, out RawFileRow row)
	{
		MDTable fileTable = FileTable;
		if (fileTable.IsInvalidRID(rid))
		{
			row = default(RawFileRow);
			return false;
		}
		DataReader reader = fileTable.DataReader;
		reader.Position = (rid - 1) * (uint)fileTable.TableInfo.RowSize;
		row = new RawFileRow(reader.Unsafe_ReadUInt32(), fileTable.Column1.Unsafe_Read24(ref reader), fileTable.Column2.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadExportedTypeRow(uint rid, out RawExportedTypeRow row)
	{
		MDTable exportedTypeTable = ExportedTypeTable;
		if (exportedTypeTable.IsInvalidRID(rid))
		{
			row = default(RawExportedTypeRow);
			return false;
		}
		DataReader reader = exportedTypeTable.DataReader;
		reader.Position = (rid - 1) * (uint)exportedTypeTable.TableInfo.RowSize;
		row = new RawExportedTypeRow(reader.Unsafe_ReadUInt32(), reader.Unsafe_ReadUInt32(), exportedTypeTable.Column2.Unsafe_Read24(ref reader), exportedTypeTable.Column3.Unsafe_Read24(ref reader), exportedTypeTable.Column4.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadManifestResourceRow(uint rid, out RawManifestResourceRow row)
	{
		MDTable manifestResourceTable = ManifestResourceTable;
		if (manifestResourceTable.IsInvalidRID(rid))
		{
			row = default(RawManifestResourceRow);
			return false;
		}
		DataReader reader = manifestResourceTable.DataReader;
		reader.Position = (rid - 1) * (uint)manifestResourceTable.TableInfo.RowSize;
		row = new RawManifestResourceRow(reader.Unsafe_ReadUInt32(), reader.Unsafe_ReadUInt32(), manifestResourceTable.Column2.Unsafe_Read24(ref reader), manifestResourceTable.Column3.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadNestedClassRow(uint rid, out RawNestedClassRow row)
	{
		MDTable nestedClassTable = NestedClassTable;
		if (nestedClassTable.IsInvalidRID(rid))
		{
			row = default(RawNestedClassRow);
			return false;
		}
		DataReader reader = nestedClassTable.DataReader;
		reader.Position = (rid - 1) * (uint)nestedClassTable.TableInfo.RowSize;
		row = new RawNestedClassRow(nestedClassTable.Column0.Unsafe_Read24(ref reader), nestedClassTable.Column1.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadGenericParamRow(uint rid, out RawGenericParamRow row)
	{
		MDTable genericParamTable = GenericParamTable;
		if (genericParamTable.IsInvalidRID(rid))
		{
			row = default(RawGenericParamRow);
			return false;
		}
		DataReader reader = genericParamTable.DataReader;
		reader.Position = (rid - 1) * (uint)genericParamTable.TableInfo.RowSize;
		if (genericParamTable.Column4 == null)
		{
			row = new RawGenericParamRow(reader.Unsafe_ReadUInt16(), reader.Unsafe_ReadUInt16(), genericParamTable.Column2.Unsafe_Read24(ref reader), genericParamTable.Column3.Unsafe_Read24(ref reader));
			return true;
		}
		row = new RawGenericParamRow(reader.Unsafe_ReadUInt16(), reader.Unsafe_ReadUInt16(), genericParamTable.Column2.Unsafe_Read24(ref reader), genericParamTable.Column3.Unsafe_Read24(ref reader), genericParamTable.Column4.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadMethodSpecRow(uint rid, out RawMethodSpecRow row)
	{
		MDTable methodSpecTable = MethodSpecTable;
		if (methodSpecTable.IsInvalidRID(rid))
		{
			row = default(RawMethodSpecRow);
			return false;
		}
		DataReader reader = methodSpecTable.DataReader;
		reader.Position = (rid - 1) * (uint)methodSpecTable.TableInfo.RowSize;
		row = new RawMethodSpecRow(methodSpecTable.Column0.Unsafe_Read24(ref reader), methodSpecTable.Column1.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadGenericParamConstraintRow(uint rid, out RawGenericParamConstraintRow row)
	{
		MDTable genericParamConstraintTable = GenericParamConstraintTable;
		if (genericParamConstraintTable.IsInvalidRID(rid))
		{
			row = default(RawGenericParamConstraintRow);
			return false;
		}
		DataReader reader = genericParamConstraintTable.DataReader;
		reader.Position = (rid - 1) * (uint)genericParamConstraintTable.TableInfo.RowSize;
		row = new RawGenericParamConstraintRow(genericParamConstraintTable.Column0.Unsafe_Read24(ref reader), genericParamConstraintTable.Column1.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadDocumentRow(uint rid, out RawDocumentRow row)
	{
		MDTable documentTable = DocumentTable;
		if (documentTable.IsInvalidRID(rid))
		{
			row = default(RawDocumentRow);
			return false;
		}
		DataReader reader = documentTable.DataReader;
		reader.Position = (rid - 1) * (uint)documentTable.TableInfo.RowSize;
		row = new RawDocumentRow(documentTable.Column0.Unsafe_Read24(ref reader), documentTable.Column1.Unsafe_Read24(ref reader), documentTable.Column2.Unsafe_Read24(ref reader), documentTable.Column3.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadMethodDebugInformationRow(uint rid, out RawMethodDebugInformationRow row)
	{
		MDTable methodDebugInformationTable = MethodDebugInformationTable;
		if (methodDebugInformationTable.IsInvalidRID(rid))
		{
			row = default(RawMethodDebugInformationRow);
			return false;
		}
		DataReader reader = methodDebugInformationTable.DataReader;
		reader.Position = (rid - 1) * (uint)methodDebugInformationTable.TableInfo.RowSize;
		row = new RawMethodDebugInformationRow(methodDebugInformationTable.Column0.Unsafe_Read24(ref reader), methodDebugInformationTable.Column1.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadLocalScopeRow(uint rid, out RawLocalScopeRow row)
	{
		MDTable localScopeTable = LocalScopeTable;
		if (localScopeTable.IsInvalidRID(rid))
		{
			row = default(RawLocalScopeRow);
			return false;
		}
		DataReader reader = localScopeTable.DataReader;
		reader.Position = (rid - 1) * (uint)localScopeTable.TableInfo.RowSize;
		row = new RawLocalScopeRow(localScopeTable.Column0.Unsafe_Read24(ref reader), localScopeTable.Column1.Unsafe_Read24(ref reader), localScopeTable.Column2.Unsafe_Read24(ref reader), localScopeTable.Column3.Unsafe_Read24(ref reader), reader.Unsafe_ReadUInt32(), reader.Unsafe_ReadUInt32());
		return true;
	}

	public bool TryReadLocalVariableRow(uint rid, out RawLocalVariableRow row)
	{
		MDTable localVariableTable = LocalVariableTable;
		if (localVariableTable.IsInvalidRID(rid))
		{
			row = default(RawLocalVariableRow);
			return false;
		}
		DataReader reader = localVariableTable.DataReader;
		reader.Position = (rid - 1) * (uint)localVariableTable.TableInfo.RowSize;
		row = new RawLocalVariableRow(reader.Unsafe_ReadUInt16(), reader.Unsafe_ReadUInt16(), localVariableTable.Column2.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadLocalConstantRow(uint rid, out RawLocalConstantRow row)
	{
		MDTable localConstantTable = LocalConstantTable;
		if (localConstantTable.IsInvalidRID(rid))
		{
			row = default(RawLocalConstantRow);
			return false;
		}
		DataReader reader = localConstantTable.DataReader;
		reader.Position = (rid - 1) * (uint)localConstantTable.TableInfo.RowSize;
		row = new RawLocalConstantRow(localConstantTable.Column0.Unsafe_Read24(ref reader), localConstantTable.Column1.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadImportScopeRow(uint rid, out RawImportScopeRow row)
	{
		MDTable importScopeTable = ImportScopeTable;
		if (importScopeTable.IsInvalidRID(rid))
		{
			row = default(RawImportScopeRow);
			return false;
		}
		DataReader reader = importScopeTable.DataReader;
		reader.Position = (rid - 1) * (uint)importScopeTable.TableInfo.RowSize;
		row = new RawImportScopeRow(importScopeTable.Column0.Unsafe_Read24(ref reader), importScopeTable.Column1.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadStateMachineMethodRow(uint rid, out RawStateMachineMethodRow row)
	{
		MDTable stateMachineMethodTable = StateMachineMethodTable;
		if (stateMachineMethodTable.IsInvalidRID(rid))
		{
			row = default(RawStateMachineMethodRow);
			return false;
		}
		DataReader reader = stateMachineMethodTable.DataReader;
		reader.Position = (rid - 1) * (uint)stateMachineMethodTable.TableInfo.RowSize;
		row = new RawStateMachineMethodRow(stateMachineMethodTable.Column0.Unsafe_Read24(ref reader), stateMachineMethodTable.Column1.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadCustomDebugInformationRow(uint rid, out RawCustomDebugInformationRow row)
	{
		MDTable customDebugInformationTable = CustomDebugInformationTable;
		if (customDebugInformationTable.IsInvalidRID(rid))
		{
			row = default(RawCustomDebugInformationRow);
			return false;
		}
		DataReader reader = customDebugInformationTable.DataReader;
		reader.Position = (rid - 1) * (uint)customDebugInformationTable.TableInfo.RowSize;
		row = new RawCustomDebugInformationRow(customDebugInformationTable.Column0.Unsafe_Read24(ref reader), customDebugInformationTable.Column1.Unsafe_Read24(ref reader), customDebugInformationTable.Column2.Unsafe_Read24(ref reader));
		return true;
	}

	public bool TryReadColumn(MDTable table, uint rid, int colIndex, out uint value)
	{
		return TryReadColumn(table, rid, table.TableInfo.Columns[colIndex], out value);
	}

	public bool TryReadColumn(MDTable table, uint rid, ColumnInfo column, out uint value)
	{
		if (table.IsInvalidRID(rid))
		{
			value = 0u;
			return false;
		}
		IColumnReader columnReader = this.columnReader;
		if (columnReader != null && columnReader.ReadColumn(table, rid, column, out value))
		{
			return true;
		}
		DataReader reader = table.DataReader;
		reader.Position = (uint)((int)(rid - 1) * table.TableInfo.RowSize + column.Offset);
		value = column.Read(ref reader);
		return true;
	}

	internal bool TryReadColumn24(MDTable table, uint rid, int colIndex, out uint value)
	{
		return TryReadColumn24(table, rid, table.TableInfo.Columns[colIndex], out value);
	}

	internal bool TryReadColumn24(MDTable table, uint rid, ColumnInfo column, out uint value)
	{
		if (table.IsInvalidRID(rid))
		{
			value = 0u;
			return false;
		}
		IColumnReader columnReader = this.columnReader;
		if (columnReader != null && columnReader.ReadColumn(table, rid, column, out value))
		{
			return true;
		}
		DataReader dataReader = table.DataReader;
		dataReader.Position = (uint)((int)(rid - 1) * table.TableInfo.RowSize + column.Offset);
		value = ((column.Size == 2) ? dataReader.Unsafe_ReadUInt16() : dataReader.Unsafe_ReadUInt32());
		return true;
	}
}
