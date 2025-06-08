using System;
using System.Collections.Generic;
using dnlib.IO;
using dnlib.PE;
using dnlib.Threading;

namespace dnlib.DotNet.MD;

internal sealed class ENCMetadata : MetadataBase
{
	private static readonly UTF8String DeletedName = "_Deleted";

	private bool hasMethodPtr;

	private bool hasFieldPtr;

	private bool hasParamPtr;

	private bool hasEventPtr;

	private bool hasPropertyPtr;

	private bool hasDeletedFields;

	private bool hasDeletedNonFields;

	private readonly CLRRuntimeReaderKind runtime;

	private readonly Dictionary<Table, SortedTable> sortedTables = new Dictionary<Table, SortedTable>();

	private readonly Lock theLock = Lock.Create();

	public override bool IsCompressed => false;

	public ENCMetadata(IPEImage peImage, ImageCor20Header cor20Header, MetadataHeader mdHeader, CLRRuntimeReaderKind runtime)
		: base(peImage, cor20Header, mdHeader)
	{
		this.runtime = runtime;
	}

	internal ENCMetadata(MetadataHeader mdHeader, bool isStandalonePortablePdb, CLRRuntimeReaderKind runtime)
		: base(mdHeader, isStandalonePortablePdb)
	{
		this.runtime = runtime;
	}

	protected override void InitializeInternal(DataReaderFactory mdReaderFactory, uint metadataBaseOffset)
	{
		DotNetStream dotNetStream = null;
		bool forceAllBig = false;
		try
		{
			if (runtime == CLRRuntimeReaderKind.Mono)
			{
				List<DotNetStream> list = new List<DotNetStream>(allStreams);
				for (int num = mdHeader.StreamHeaders.Count - 1; num >= 0; num--)
				{
					StreamHeader streamHeader = mdHeader.StreamHeaders[num];
					switch (streamHeader.Name)
					{
					case "#Strings":
						if (stringsStream == null)
						{
							stringsStream = new StringsStream(mdReaderFactory, metadataBaseOffset, streamHeader);
							list.Add(stringsStream);
							continue;
						}
						break;
					case "#US":
						if (usStream == null)
						{
							usStream = new USStream(mdReaderFactory, metadataBaseOffset, streamHeader);
							list.Add(usStream);
							continue;
						}
						break;
					case "#Blob":
						if (blobStream == null)
						{
							blobStream = new BlobStream(mdReaderFactory, metadataBaseOffset, streamHeader);
							list.Add(blobStream);
							continue;
						}
						break;
					case "#GUID":
						if (guidStream == null)
						{
							guidStream = new GuidStream(mdReaderFactory, metadataBaseOffset, streamHeader);
							list.Add(guidStream);
							continue;
						}
						break;
					case "#-":
					case "#~":
						if (tablesStream == null)
						{
							tablesStream = new TablesStream(mdReaderFactory, metadataBaseOffset, streamHeader, runtime);
							list.Add(tablesStream);
							continue;
						}
						break;
					case "#Pdb":
						if (isStandalonePortablePdb && pdbStream == null)
						{
							pdbStream = new PdbStream(mdReaderFactory, metadataBaseOffset, streamHeader);
							list.Add(pdbStream);
							continue;
						}
						break;
					case "#JTD":
						forceAllBig = true;
						continue;
					}
					dotNetStream = new CustomDotNetStream(mdReaderFactory, metadataBaseOffset, streamHeader);
					list.Add(dotNetStream);
					dotNetStream = null;
				}
				list.Reverse();
				allStreams = list;
			}
			else
			{
				foreach (StreamHeader streamHeader2 in mdHeader.StreamHeaders)
				{
					switch (streamHeader2.Name.ToUpperInvariant())
					{
					case "#STRINGS":
						if (stringsStream == null)
						{
							stringsStream = new StringsStream(mdReaderFactory, metadataBaseOffset, streamHeader2);
							allStreams.Add(stringsStream);
							continue;
						}
						break;
					case "#US":
						if (usStream == null)
						{
							usStream = new USStream(mdReaderFactory, metadataBaseOffset, streamHeader2);
							allStreams.Add(usStream);
							continue;
						}
						break;
					case "#BLOB":
						if (blobStream == null)
						{
							blobStream = new BlobStream(mdReaderFactory, metadataBaseOffset, streamHeader2);
							allStreams.Add(blobStream);
							continue;
						}
						break;
					case "#GUID":
						if (guidStream == null)
						{
							guidStream = new GuidStream(mdReaderFactory, metadataBaseOffset, streamHeader2);
							allStreams.Add(guidStream);
							continue;
						}
						break;
					case "#-":
					case "#~":
						if (tablesStream == null)
						{
							tablesStream = new TablesStream(mdReaderFactory, metadataBaseOffset, streamHeader2, runtime);
							allStreams.Add(tablesStream);
							continue;
						}
						break;
					case "#PDB":
						if (isStandalonePortablePdb && pdbStream == null && streamHeader2.Name == "#Pdb")
						{
							pdbStream = new PdbStream(mdReaderFactory, metadataBaseOffset, streamHeader2);
							allStreams.Add(pdbStream);
							continue;
						}
						break;
					case "#JTD":
						forceAllBig = true;
						continue;
					}
					dotNetStream = new CustomDotNetStream(mdReaderFactory, metadataBaseOffset, streamHeader2);
					allStreams.Add(dotNetStream);
					dotNetStream = null;
				}
			}
		}
		finally
		{
			dotNetStream?.Dispose();
		}
		if (tablesStream == null)
		{
			throw new BadImageFormatException("Missing MD stream");
		}
		if (pdbStream != null)
		{
			tablesStream.Initialize(pdbStream.TypeSystemTableRows, forceAllBig);
		}
		else
		{
			tablesStream.Initialize(null, forceAllBig);
		}
		hasFieldPtr = !tablesStream.FieldPtrTable.IsEmpty;
		hasMethodPtr = !tablesStream.MethodPtrTable.IsEmpty;
		hasParamPtr = !tablesStream.ParamPtrTable.IsEmpty;
		hasEventPtr = !tablesStream.EventPtrTable.IsEmpty;
		hasPropertyPtr = !tablesStream.PropertyPtrTable.IsEmpty;
		switch (runtime)
		{
		case CLRRuntimeReaderKind.CLR:
			hasDeletedFields = tablesStream.HasDelete;
			hasDeletedNonFields = tablesStream.HasDelete;
			break;
		case CLRRuntimeReaderKind.Mono:
			hasDeletedFields = true;
			hasDeletedNonFields = false;
			break;
		default:
			throw new InvalidOperationException();
		}
	}

	public override RidList GetTypeDefRidList()
	{
		if (!hasDeletedNonFields)
		{
			return base.GetTypeDefRidList();
		}
		uint rows = tablesStream.TypeDefTable.Rows;
		List<uint> list = new List<uint>((int)rows);
		for (uint num = 1u; num <= rows; num++)
		{
			if (tablesStream.TryReadTypeDefRow(num, out var row) && (num == 1 || !stringsStream.ReadNoNull(row.Name).StartsWith(DeletedName)))
			{
				list.Add(num);
			}
		}
		return RidList.Create(list);
	}

	public override RidList GetExportedTypeRidList()
	{
		if (!hasDeletedNonFields)
		{
			return base.GetExportedTypeRidList();
		}
		uint rows = tablesStream.ExportedTypeTable.Rows;
		List<uint> list = new List<uint>((int)rows);
		for (uint num = 1u; num <= rows; num++)
		{
			if (tablesStream.TryReadExportedTypeRow(num, out var row) && !stringsStream.ReadNoNull(row.TypeName).StartsWith(DeletedName))
			{
				list.Add(num);
			}
		}
		return RidList.Create(list);
	}

	private uint ToFieldRid(uint listRid)
	{
		if (!hasFieldPtr)
		{
			return listRid;
		}
		if (!tablesStream.TryReadColumn24(tablesStream.FieldPtrTable, listRid, 0, out var value))
		{
			return 0u;
		}
		return value;
	}

	private uint ToMethodRid(uint listRid)
	{
		if (!hasMethodPtr)
		{
			return listRid;
		}
		if (!tablesStream.TryReadColumn24(tablesStream.MethodPtrTable, listRid, 0, out var value))
		{
			return 0u;
		}
		return value;
	}

	private uint ToParamRid(uint listRid)
	{
		if (!hasParamPtr)
		{
			return listRid;
		}
		if (!tablesStream.TryReadColumn24(tablesStream.ParamPtrTable, listRid, 0, out var value))
		{
			return 0u;
		}
		return value;
	}

	private uint ToEventRid(uint listRid)
	{
		if (!hasEventPtr)
		{
			return listRid;
		}
		if (!tablesStream.TryReadColumn24(tablesStream.EventPtrTable, listRid, 0, out var value))
		{
			return 0u;
		}
		return value;
	}

	private uint ToPropertyRid(uint listRid)
	{
		if (!hasPropertyPtr)
		{
			return listRid;
		}
		if (!tablesStream.TryReadColumn24(tablesStream.PropertyPtrTable, listRid, 0, out var value))
		{
			return 0u;
		}
		return value;
	}

	public override RidList GetFieldRidList(uint typeDefRid)
	{
		RidList ridList = GetRidList(tablesStream.TypeDefTable, typeDefRid, 4, tablesStream.FieldTable);
		if (ridList.Count == 0 || (!hasFieldPtr && !hasDeletedFields))
		{
			return ridList;
		}
		MDTable fieldTable = tablesStream.FieldTable;
		List<uint> list = new List<uint>(ridList.Count);
		for (int i = 0; i < ridList.Count; i++)
		{
			uint num = ToFieldRid(ridList[i]);
			if (fieldTable.IsInvalidRID(num))
			{
				continue;
			}
			if (hasDeletedFields)
			{
				if (!tablesStream.TryReadFieldRow(num, out var row))
				{
					continue;
				}
				if (runtime == CLRRuntimeReaderKind.CLR)
				{
					if ((row.Flags & 0x400u) != 0 && stringsStream.ReadNoNull(row.Name).StartsWith(DeletedName))
					{
						continue;
					}
				}
				else if ((row.Flags & 0x600) == 1536 && stringsStream.ReadNoNull(row.Name) == DeletedName)
				{
					continue;
				}
			}
			list.Add(num);
		}
		return RidList.Create(list);
	}

	public override RidList GetMethodRidList(uint typeDefRid)
	{
		RidList ridList = GetRidList(tablesStream.TypeDefTable, typeDefRid, 5, tablesStream.MethodTable);
		if (ridList.Count == 0 || (!hasMethodPtr && !hasDeletedNonFields))
		{
			return ridList;
		}
		MDTable methodTable = tablesStream.MethodTable;
		List<uint> list = new List<uint>(ridList.Count);
		for (int i = 0; i < ridList.Count; i++)
		{
			uint num = ToMethodRid(ridList[i]);
			if (!methodTable.IsInvalidRID(num) && (!hasDeletedNonFields || (tablesStream.TryReadMethodRow(num, out var row) && ((row.Flags & 0x1000) == 0 || !stringsStream.ReadNoNull(row.Name).StartsWith(DeletedName)))))
			{
				list.Add(num);
			}
		}
		return RidList.Create(list);
	}

	public override RidList GetParamRidList(uint methodRid)
	{
		RidList ridList = GetRidList(tablesStream.MethodTable, methodRid, 5, tablesStream.ParamTable);
		if (ridList.Count == 0 || !hasParamPtr)
		{
			return ridList;
		}
		MDTable paramTable = tablesStream.ParamTable;
		List<uint> list = new List<uint>(ridList.Count);
		for (int i = 0; i < ridList.Count; i++)
		{
			uint num = ToParamRid(ridList[i]);
			if (!paramTable.IsInvalidRID(num))
			{
				list.Add(num);
			}
		}
		return RidList.Create(list);
	}

	public override RidList GetEventRidList(uint eventMapRid)
	{
		RidList ridList = GetRidList(tablesStream.EventMapTable, eventMapRid, 1, tablesStream.EventTable);
		if (ridList.Count == 0 || (!hasEventPtr && !hasDeletedNonFields))
		{
			return ridList;
		}
		MDTable eventTable = tablesStream.EventTable;
		List<uint> list = new List<uint>(ridList.Count);
		for (int i = 0; i < ridList.Count; i++)
		{
			uint num = ToEventRid(ridList[i]);
			if (!eventTable.IsInvalidRID(num) && (!hasDeletedNonFields || (tablesStream.TryReadEventRow(num, out var row) && ((row.EventFlags & 0x400) == 0 || !stringsStream.ReadNoNull(row.Name).StartsWith(DeletedName)))))
			{
				list.Add(num);
			}
		}
		return RidList.Create(list);
	}

	public override RidList GetPropertyRidList(uint propertyMapRid)
	{
		RidList ridList = GetRidList(tablesStream.PropertyMapTable, propertyMapRid, 1, tablesStream.PropertyTable);
		if (ridList.Count == 0 || (!hasPropertyPtr && !hasDeletedNonFields))
		{
			return ridList;
		}
		MDTable propertyTable = tablesStream.PropertyTable;
		List<uint> list = new List<uint>(ridList.Count);
		for (int i = 0; i < ridList.Count; i++)
		{
			uint num = ToPropertyRid(ridList[i]);
			if (!propertyTable.IsInvalidRID(num) && (!hasDeletedNonFields || (tablesStream.TryReadPropertyRow(num, out var row) && ((row.PropFlags & 0x400) == 0 || !stringsStream.ReadNoNull(row.Name).StartsWith(DeletedName)))))
			{
				list.Add(num);
			}
		}
		return RidList.Create(list);
	}

	public override RidList GetLocalVariableRidList(uint localScopeRid)
	{
		return GetRidList(tablesStream.LocalScopeTable, localScopeRid, 2, tablesStream.LocalVariableTable);
	}

	public override RidList GetLocalConstantRidList(uint localScopeRid)
	{
		return GetRidList(tablesStream.LocalScopeTable, localScopeRid, 3, tablesStream.LocalConstantTable);
	}

	private RidList GetRidList(MDTable tableSource, uint tableSourceRid, int colIndex, MDTable tableDest)
	{
		ColumnInfo column = tableSource.TableInfo.Columns[colIndex];
		if (!tablesStream.TryReadColumn24(tableSource, tableSourceRid, column, out var value))
		{
			return RidList.Empty;
		}
		uint value2;
		bool flag = tablesStream.TryReadColumn24(tableSource, tableSourceRid + 1, column, out value2);
		uint num = tableDest.Rows + 1;
		if (value == 0 || value >= num)
		{
			return RidList.Empty;
		}
		uint num2 = ((flag && value2 != 0) ? value2 : num);
		if (num2 < value)
		{
			num2 = value;
		}
		if (num2 > num)
		{
			num2 = num;
		}
		return RidList.Create(value, num2 - value);
	}

	protected override uint BinarySearch(MDTable tableSource, int keyColIndex, uint key)
	{
		ColumnInfo column = tableSource.TableInfo.Columns[keyColIndex];
		uint num = 1u;
		uint num2 = tableSource.Rows;
		while (num <= num2)
		{
			uint num3 = (num + num2) / 2;
			if (!tablesStream.TryReadColumn24(tableSource, num3, column, out var value))
			{
				break;
			}
			if (key == value)
			{
				return num3;
			}
			if (value > key)
			{
				num2 = num3 - 1;
			}
			else
			{
				num = num3 + 1;
			}
		}
		if (tableSource.Table == Table.GenericParam && !tablesStream.IsSorted(tableSource))
		{
			return LinearSearch(tableSource, keyColIndex, key);
		}
		return 0u;
	}

	private uint LinearSearch(MDTable tableSource, int keyColIndex, uint key)
	{
		if (tableSource == null)
		{
			return 0u;
		}
		ColumnInfo column = tableSource.TableInfo.Columns[keyColIndex];
		uint value;
		for (uint num = 1u; num <= tableSource.Rows && tablesStream.TryReadColumn24(tableSource, num, column, out value); num++)
		{
			if (key == value)
			{
				return num;
			}
		}
		return 0u;
	}

	protected override RidList FindAllRowsUnsorted(MDTable tableSource, int keyColIndex, uint key)
	{
		if (tablesStream.IsSorted(tableSource))
		{
			return FindAllRows(tableSource, keyColIndex, key);
		}
		theLock.EnterWriteLock();
		SortedTable value;
		try
		{
			if (!sortedTables.TryGetValue(tableSource.Table, out value))
			{
				value = (sortedTables[tableSource.Table] = new SortedTable(tableSource, keyColIndex));
			}
		}
		finally
		{
			theLock.ExitWriteLock();
		}
		return value.FindAllRows(key);
	}
}
