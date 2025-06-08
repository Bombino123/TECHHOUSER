using System;
using System.Collections.Generic;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.MD;

internal sealed class CompressedMetadata : MetadataBase
{
	private readonly CLRRuntimeReaderKind runtime;

	public override bool IsCompressed => true;

	public CompressedMetadata(IPEImage peImage, ImageCor20Header cor20Header, MetadataHeader mdHeader, CLRRuntimeReaderKind runtime)
		: base(peImage, cor20Header, mdHeader)
	{
		this.runtime = runtime;
	}

	internal CompressedMetadata(MetadataHeader mdHeader, bool isStandalonePortablePdb, CLRRuntimeReaderKind runtime)
		: base(mdHeader, isStandalonePortablePdb)
	{
		this.runtime = runtime;
	}

	protected override void InitializeInternal(DataReaderFactory mdReaderFactory, uint metadataBaseOffset)
	{
		DotNetStream dotNetStream = null;
		List<DotNetStream> list = new List<DotNetStream>(allStreams);
		bool forceAllBig = false;
		try
		{
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
					if (runtime == CLRRuntimeReaderKind.Mono)
					{
						forceAllBig = true;
						continue;
					}
					break;
				}
				dotNetStream = new CustomDotNetStream(mdReaderFactory, metadataBaseOffset, streamHeader);
				list.Add(dotNetStream);
				dotNetStream = null;
			}
		}
		finally
		{
			dotNetStream?.Dispose();
			list.Reverse();
			allStreams = list;
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
	}

	public override RidList GetFieldRidList(uint typeDefRid)
	{
		return GetRidList(tablesStream.TypeDefTable, typeDefRid, 4, tablesStream.FieldTable);
	}

	public override RidList GetMethodRidList(uint typeDefRid)
	{
		return GetRidList(tablesStream.TypeDefTable, typeDefRid, 5, tablesStream.MethodTable);
	}

	public override RidList GetParamRidList(uint methodRid)
	{
		return GetRidList(tablesStream.MethodTable, methodRid, 5, tablesStream.ParamTable);
	}

	public override RidList GetEventRidList(uint eventMapRid)
	{
		return GetRidList(tablesStream.EventMapTable, eventMapRid, 1, tablesStream.EventTable);
	}

	public override RidList GetPropertyRidList(uint propertyMapRid)
	{
		return GetRidList(tablesStream.PropertyMapTable, propertyMapRid, 1, tablesStream.PropertyTable);
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
		uint num2 = ((!flag || (value2 == 0 && tableSourceRid + 1 == tableSource.Rows && tableDest.Rows == 65535)) ? num : value2);
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
		return 0u;
	}
}
