using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.MD;

internal abstract class MetadataBase : Metadata
{
	protected sealed class SortedTable
	{
		[DebuggerDisplay("{rid} {key}")]
		private readonly struct RowInfo : IComparable<RowInfo>
		{
			public readonly uint rid;

			public readonly uint key;

			public RowInfo(uint rid, uint key)
			{
				this.rid = rid;
				this.key = key;
			}

			public int CompareTo(RowInfo other)
			{
				if (key < other.key)
				{
					return -1;
				}
				if (key > other.key)
				{
					return 1;
				}
				uint num = rid;
				return num.CompareTo(other.rid);
			}
		}

		private RowInfo[] rows;

		public SortedTable(MDTable mdTable, int keyColIndex)
		{
			InitializeKeys(mdTable, keyColIndex);
			Array.Sort(rows);
		}

		private void InitializeKeys(MDTable mdTable, int keyColIndex)
		{
			ColumnInfo columnInfo = mdTable.TableInfo.Columns[keyColIndex];
			rows = new RowInfo[mdTable.Rows + 1];
			if (mdTable.Rows == 0)
			{
				return;
			}
			DataReader reader = mdTable.DataReader;
			reader.Position = (uint)columnInfo.Offset;
			uint num = (uint)(mdTable.TableInfo.RowSize - columnInfo.Size);
			for (uint num2 = 1u; num2 <= mdTable.Rows; num2++)
			{
				rows[num2] = new RowInfo(num2, columnInfo.Unsafe_Read24(ref reader));
				if (num2 < mdTable.Rows)
				{
					reader.Position += num;
				}
			}
		}

		private int BinarySearch(uint key)
		{
			int num = 1;
			int num2 = rows.Length - 1;
			while (num <= num2 && num2 != -1)
			{
				int num3 = (num + num2) / 2;
				uint key2 = rows[num3].key;
				if (key == key2)
				{
					return num3;
				}
				if (key2 > key)
				{
					num2 = num3 - 1;
				}
				else
				{
					num = num3 + 1;
				}
			}
			return 0;
		}

		public RidList FindAllRows(uint key)
		{
			int num = BinarySearch(key);
			if (num == 0)
			{
				return RidList.Empty;
			}
			int i = num + 1;
			while (num > 1 && key == rows[num - 1].key)
			{
				num--;
			}
			for (; i < rows.Length && key == rows[i].key; i++)
			{
			}
			List<uint> list = new List<uint>(i - num);
			for (int j = num; j < i; j++)
			{
				list.Add(rows[j].rid);
			}
			return RidList.Create(list);
		}
	}

	protected IPEImage peImage;

	protected ImageCor20Header cor20Header;

	protected MetadataHeader mdHeader;

	protected StringsStream stringsStream;

	protected USStream usStream;

	protected BlobStream blobStream;

	protected GuidStream guidStream;

	protected TablesStream tablesStream;

	protected PdbStream pdbStream;

	protected IList<DotNetStream> allStreams;

	protected readonly bool isStandalonePortablePdb;

	private uint[] fieldRidToTypeDefRid;

	private uint[] methodRidToTypeDefRid;

	private uint[] eventRidToTypeDefRid;

	private uint[] propertyRidToTypeDefRid;

	private uint[] gpRidToOwnerRid;

	private uint[] gpcRidToOwnerRid;

	private uint[] paramRidToOwnerRid;

	private Dictionary<uint, List<uint>> typeDefRidToNestedClasses;

	private StrongBox<RidList> nonNestedTypes;

	private DataReaderFactory mdReaderFactoryToDisposeLater;

	private SortedTable eventMapSortedTable;

	private SortedTable propertyMapSortedTable;

	public override bool IsStandalonePortablePdb => isStandalonePortablePdb;

	public override ImageCor20Header ImageCor20Header => cor20Header;

	public override uint Version => (uint)((mdHeader.MajorVersion << 16) | mdHeader.MinorVersion);

	public override string VersionString => mdHeader.VersionString;

	public override IPEImage PEImage => peImage;

	public override MetadataHeader MetadataHeader => mdHeader;

	public override StringsStream StringsStream => stringsStream;

	public override USStream USStream => usStream;

	public override BlobStream BlobStream => blobStream;

	public override GuidStream GuidStream => guidStream;

	public override TablesStream TablesStream => tablesStream;

	public override PdbStream PdbStream => pdbStream;

	public override IList<DotNetStream> AllStreams => allStreams;

	protected MetadataBase(IPEImage peImage, ImageCor20Header cor20Header, MetadataHeader mdHeader)
	{
		try
		{
			allStreams = new List<DotNetStream>();
			this.peImage = peImage;
			this.cor20Header = cor20Header;
			this.mdHeader = mdHeader;
			isStandalonePortablePdb = false;
		}
		catch
		{
			peImage?.Dispose();
			throw;
		}
	}

	internal MetadataBase(MetadataHeader mdHeader, bool isStandalonePortablePdb)
	{
		allStreams = new List<DotNetStream>();
		peImage = null;
		cor20Header = null;
		this.mdHeader = mdHeader;
		this.isStandalonePortablePdb = isStandalonePortablePdb;
	}

	public void Initialize(DataReaderFactory mdReaderFactory)
	{
		mdReaderFactoryToDisposeLater = mdReaderFactory;
		uint metadataBaseOffset;
		if (peImage != null)
		{
			metadataBaseOffset = (uint)peImage.ToFileOffset(cor20Header.Metadata.VirtualAddress);
			mdReaderFactory = peImage.DataReaderFactory;
		}
		else
		{
			metadataBaseOffset = 0u;
		}
		InitializeInternal(mdReaderFactory, metadataBaseOffset);
		if (tablesStream == null)
		{
			throw new BadImageFormatException("Missing MD stream");
		}
		if (isStandalonePortablePdb && pdbStream == null)
		{
			throw new BadImageFormatException("Missing #Pdb stream");
		}
		InitializeNonExistentHeaps();
	}

	protected void InitializeNonExistentHeaps()
	{
		if (stringsStream == null)
		{
			stringsStream = new StringsStream();
		}
		if (usStream == null)
		{
			usStream = new USStream();
		}
		if (blobStream == null)
		{
			blobStream = new BlobStream();
		}
		if (guidStream == null)
		{
			guidStream = new GuidStream();
		}
	}

	protected abstract void InitializeInternal(DataReaderFactory mdReaderFactory, uint metadataBaseOffset);

	public override RidList GetTypeDefRidList()
	{
		return RidList.Create(1u, tablesStream.TypeDefTable.Rows);
	}

	public override RidList GetExportedTypeRidList()
	{
		return RidList.Create(1u, tablesStream.ExportedTypeTable.Rows);
	}

	protected abstract uint BinarySearch(MDTable tableSource, int keyColIndex, uint key);

	protected RidList FindAllRows(MDTable tableSource, int keyColIndex, uint key)
	{
		uint num = BinarySearch(tableSource, keyColIndex, key);
		if (tableSource.IsInvalidRID(num))
		{
			return RidList.Empty;
		}
		uint num2 = num + 1;
		ColumnInfo column = tableSource.TableInfo.Columns[keyColIndex];
		uint value;
		while (num > 1 && tablesStream.TryReadColumn24(tableSource, num - 1, column, out value) && key == value)
		{
			num--;
		}
		uint value2;
		for (; num2 <= tableSource.Rows && tablesStream.TryReadColumn24(tableSource, num2, column, out value2); num2++)
		{
			if (key != value2)
			{
				break;
			}
		}
		return RidList.Create(num, num2 - num);
	}

	protected virtual RidList FindAllRowsUnsorted(MDTable tableSource, int keyColIndex, uint key)
	{
		return FindAllRows(tableSource, keyColIndex, key);
	}

	public override RidList GetInterfaceImplRidList(uint typeDefRid)
	{
		return FindAllRowsUnsorted(tablesStream.InterfaceImplTable, 0, typeDefRid);
	}

	public override RidList GetGenericParamRidList(Table table, uint rid)
	{
		if (!CodedToken.TypeOrMethodDef.Encode(new MDToken(table, rid), out var codedToken))
		{
			return RidList.Empty;
		}
		return FindAllRowsUnsorted(tablesStream.GenericParamTable, 2, codedToken);
	}

	public override RidList GetGenericParamConstraintRidList(uint genericParamRid)
	{
		return FindAllRowsUnsorted(tablesStream.GenericParamConstraintTable, 0, genericParamRid);
	}

	public override RidList GetCustomAttributeRidList(Table table, uint rid)
	{
		if (!CodedToken.HasCustomAttribute.Encode(new MDToken(table, rid), out var codedToken))
		{
			return RidList.Empty;
		}
		return FindAllRowsUnsorted(tablesStream.CustomAttributeTable, 0, codedToken);
	}

	public override RidList GetDeclSecurityRidList(Table table, uint rid)
	{
		if (!CodedToken.HasDeclSecurity.Encode(new MDToken(table, rid), out var codedToken))
		{
			return RidList.Empty;
		}
		return FindAllRowsUnsorted(tablesStream.DeclSecurityTable, 1, codedToken);
	}

	public override RidList GetMethodSemanticsRidList(Table table, uint rid)
	{
		if (!CodedToken.HasSemantic.Encode(new MDToken(table, rid), out var codedToken))
		{
			return RidList.Empty;
		}
		return FindAllRowsUnsorted(tablesStream.MethodSemanticsTable, 2, codedToken);
	}

	public override RidList GetMethodImplRidList(uint typeDefRid)
	{
		return FindAllRowsUnsorted(tablesStream.MethodImplTable, 0, typeDefRid);
	}

	public override uint GetClassLayoutRid(uint typeDefRid)
	{
		RidList ridList = FindAllRowsUnsorted(tablesStream.ClassLayoutTable, 2, typeDefRid);
		if (ridList.Count != 0)
		{
			return ridList[0];
		}
		return 0u;
	}

	public override uint GetFieldLayoutRid(uint fieldRid)
	{
		RidList ridList = FindAllRowsUnsorted(tablesStream.FieldLayoutTable, 1, fieldRid);
		if (ridList.Count != 0)
		{
			return ridList[0];
		}
		return 0u;
	}

	public override uint GetFieldMarshalRid(Table table, uint rid)
	{
		if (!CodedToken.HasFieldMarshal.Encode(new MDToken(table, rid), out var codedToken))
		{
			return 0u;
		}
		RidList ridList = FindAllRowsUnsorted(tablesStream.FieldMarshalTable, 0, codedToken);
		if (ridList.Count != 0)
		{
			return ridList[0];
		}
		return 0u;
	}

	public override uint GetFieldRVARid(uint fieldRid)
	{
		RidList ridList = FindAllRowsUnsorted(tablesStream.FieldRVATable, 1, fieldRid);
		if (ridList.Count != 0)
		{
			return ridList[0];
		}
		return 0u;
	}

	public override uint GetImplMapRid(Table table, uint rid)
	{
		if (!CodedToken.MemberForwarded.Encode(new MDToken(table, rid), out var codedToken))
		{
			return 0u;
		}
		RidList ridList = FindAllRowsUnsorted(tablesStream.ImplMapTable, 1, codedToken);
		if (ridList.Count != 0)
		{
			return ridList[0];
		}
		return 0u;
	}

	public override uint GetNestedClassRid(uint typeDefRid)
	{
		RidList ridList = FindAllRowsUnsorted(tablesStream.NestedClassTable, 0, typeDefRid);
		if (ridList.Count != 0)
		{
			return ridList[0];
		}
		return 0u;
	}

	public override uint GetEventMapRid(uint typeDefRid)
	{
		if (eventMapSortedTable == null)
		{
			Interlocked.CompareExchange(ref eventMapSortedTable, new SortedTable(tablesStream.EventMapTable, 0), null);
		}
		RidList ridList = eventMapSortedTable.FindAllRows(typeDefRid);
		if (ridList.Count != 0)
		{
			return ridList[0];
		}
		return 0u;
	}

	public override uint GetPropertyMapRid(uint typeDefRid)
	{
		if (propertyMapSortedTable == null)
		{
			Interlocked.CompareExchange(ref propertyMapSortedTable, new SortedTable(tablesStream.PropertyMapTable, 0), null);
		}
		RidList ridList = propertyMapSortedTable.FindAllRows(typeDefRid);
		if (ridList.Count != 0)
		{
			return ridList[0];
		}
		return 0u;
	}

	public override uint GetConstantRid(Table table, uint rid)
	{
		if (!CodedToken.HasConstant.Encode(new MDToken(table, rid), out var codedToken))
		{
			return 0u;
		}
		RidList ridList = FindAllRowsUnsorted(tablesStream.ConstantTable, 2, codedToken);
		if (ridList.Count != 0)
		{
			return ridList[0];
		}
		return 0u;
	}

	public override uint GetOwnerTypeOfField(uint fieldRid)
	{
		if (fieldRidToTypeDefRid == null)
		{
			InitializeInverseFieldOwnerRidList();
		}
		uint num = fieldRid - 1;
		if (num >= fieldRidToTypeDefRid.LongLength)
		{
			return 0u;
		}
		return fieldRidToTypeDefRid[num];
	}

	private void InitializeInverseFieldOwnerRidList()
	{
		if (fieldRidToTypeDefRid != null)
		{
			return;
		}
		uint[] array = new uint[tablesStream.FieldTable.Rows];
		RidList typeDefRidList = GetTypeDefRidList();
		for (int i = 0; i < typeDefRidList.Count; i++)
		{
			uint num = typeDefRidList[i];
			RidList fieldRidList = GetFieldRidList(num);
			for (int j = 0; j < fieldRidList.Count; j++)
			{
				uint num2 = fieldRidList[j] - 1;
				if (array[num2] == 0)
				{
					array[num2] = num;
				}
			}
		}
		Interlocked.CompareExchange(ref fieldRidToTypeDefRid, array, null);
	}

	public override uint GetOwnerTypeOfMethod(uint methodRid)
	{
		if (methodRidToTypeDefRid == null)
		{
			InitializeInverseMethodOwnerRidList();
		}
		uint num = methodRid - 1;
		if (num >= methodRidToTypeDefRid.LongLength)
		{
			return 0u;
		}
		return methodRidToTypeDefRid[num];
	}

	private void InitializeInverseMethodOwnerRidList()
	{
		if (methodRidToTypeDefRid != null)
		{
			return;
		}
		uint[] array = new uint[tablesStream.MethodTable.Rows];
		RidList typeDefRidList = GetTypeDefRidList();
		for (int i = 0; i < typeDefRidList.Count; i++)
		{
			uint num = typeDefRidList[i];
			RidList methodRidList = GetMethodRidList(num);
			for (int j = 0; j < methodRidList.Count; j++)
			{
				uint num2 = methodRidList[j] - 1;
				if (array[num2] == 0)
				{
					array[num2] = num;
				}
			}
		}
		Interlocked.CompareExchange(ref methodRidToTypeDefRid, array, null);
	}

	public override uint GetOwnerTypeOfEvent(uint eventRid)
	{
		if (eventRidToTypeDefRid == null)
		{
			InitializeInverseEventOwnerRidList();
		}
		uint num = eventRid - 1;
		if (num >= eventRidToTypeDefRid.LongLength)
		{
			return 0u;
		}
		return eventRidToTypeDefRid[num];
	}

	private void InitializeInverseEventOwnerRidList()
	{
		if (eventRidToTypeDefRid != null)
		{
			return;
		}
		uint[] array = new uint[tablesStream.EventTable.Rows];
		RidList typeDefRidList = GetTypeDefRidList();
		for (int i = 0; i < typeDefRidList.Count; i++)
		{
			uint num = typeDefRidList[i];
			RidList eventRidList = GetEventRidList(GetEventMapRid(num));
			for (int j = 0; j < eventRidList.Count; j++)
			{
				uint num2 = eventRidList[j] - 1;
				if (array[num2] == 0)
				{
					array[num2] = num;
				}
			}
		}
		Interlocked.CompareExchange(ref eventRidToTypeDefRid, array, null);
	}

	public override uint GetOwnerTypeOfProperty(uint propertyRid)
	{
		if (propertyRidToTypeDefRid == null)
		{
			InitializeInversePropertyOwnerRidList();
		}
		uint num = propertyRid - 1;
		if (num >= propertyRidToTypeDefRid.LongLength)
		{
			return 0u;
		}
		return propertyRidToTypeDefRid[num];
	}

	private void InitializeInversePropertyOwnerRidList()
	{
		if (propertyRidToTypeDefRid != null)
		{
			return;
		}
		uint[] array = new uint[tablesStream.PropertyTable.Rows];
		RidList typeDefRidList = GetTypeDefRidList();
		for (int i = 0; i < typeDefRidList.Count; i++)
		{
			uint num = typeDefRidList[i];
			RidList propertyRidList = GetPropertyRidList(GetPropertyMapRid(num));
			for (int j = 0; j < propertyRidList.Count; j++)
			{
				uint num2 = propertyRidList[j] - 1;
				if (array[num2] == 0)
				{
					array[num2] = num;
				}
			}
		}
		Interlocked.CompareExchange(ref propertyRidToTypeDefRid, array, null);
	}

	public override uint GetOwnerOfGenericParam(uint gpRid)
	{
		if (gpRidToOwnerRid == null)
		{
			InitializeInverseGenericParamOwnerRidList();
		}
		uint num = gpRid - 1;
		if (num >= gpRidToOwnerRid.LongLength)
		{
			return 0u;
		}
		return gpRidToOwnerRid[num];
	}

	private void InitializeInverseGenericParamOwnerRidList()
	{
		if (gpRidToOwnerRid != null)
		{
			return;
		}
		MDTable genericParamTable = tablesStream.GenericParamTable;
		uint[] array = new uint[genericParamTable.Rows];
		ColumnInfo column = genericParamTable.TableInfo.Columns[2];
		Dictionary<uint, bool> dictionary = new Dictionary<uint, bool>();
		for (uint num = 1u; num <= genericParamTable.Rows; num++)
		{
			if (tablesStream.TryReadColumn24(genericParamTable, num, column, out var value))
			{
				dictionary[value] = true;
			}
		}
		List<uint> list = new List<uint>(dictionary.Keys);
		list.Sort();
		for (int i = 0; i < list.Count; i++)
		{
			if (!CodedToken.TypeOrMethodDef.Decode(list[i], out uint token))
			{
				continue;
			}
			RidList genericParamRidList = GetGenericParamRidList(MDToken.ToTable(token), MDToken.ToRID(token));
			for (int j = 0; j < genericParamRidList.Count; j++)
			{
				uint num2 = genericParamRidList[j] - 1;
				if (array[num2] == 0)
				{
					array[num2] = list[i];
				}
			}
		}
		Interlocked.CompareExchange(ref gpRidToOwnerRid, array, null);
	}

	public override uint GetOwnerOfGenericParamConstraint(uint gpcRid)
	{
		if (gpcRidToOwnerRid == null)
		{
			InitializeInverseGenericParamConstraintOwnerRidList();
		}
		uint num = gpcRid - 1;
		if (num >= gpcRidToOwnerRid.LongLength)
		{
			return 0u;
		}
		return gpcRidToOwnerRid[num];
	}

	private void InitializeInverseGenericParamConstraintOwnerRidList()
	{
		if (gpcRidToOwnerRid != null)
		{
			return;
		}
		MDTable genericParamConstraintTable = tablesStream.GenericParamConstraintTable;
		uint[] array = new uint[genericParamConstraintTable.Rows];
		ColumnInfo column = genericParamConstraintTable.TableInfo.Columns[0];
		Dictionary<uint, bool> dictionary = new Dictionary<uint, bool>();
		for (uint num = 1u; num <= genericParamConstraintTable.Rows; num++)
		{
			if (tablesStream.TryReadColumn24(genericParamConstraintTable, num, column, out var value))
			{
				dictionary[value] = true;
			}
		}
		List<uint> list = new List<uint>(dictionary.Keys);
		list.Sort();
		for (int i = 0; i < list.Count; i++)
		{
			uint num2 = list[i];
			RidList genericParamConstraintRidList = GetGenericParamConstraintRidList(num2);
			for (int j = 0; j < genericParamConstraintRidList.Count; j++)
			{
				uint num3 = genericParamConstraintRidList[j] - 1;
				if (array[num3] == 0)
				{
					array[num3] = num2;
				}
			}
		}
		Interlocked.CompareExchange(ref gpcRidToOwnerRid, array, null);
	}

	public override uint GetOwnerOfParam(uint paramRid)
	{
		if (paramRidToOwnerRid == null)
		{
			InitializeInverseParamOwnerRidList();
		}
		uint num = paramRid - 1;
		if (num >= paramRidToOwnerRid.LongLength)
		{
			return 0u;
		}
		return paramRidToOwnerRid[num];
	}

	private void InitializeInverseParamOwnerRidList()
	{
		if (paramRidToOwnerRid != null)
		{
			return;
		}
		uint[] array = new uint[tablesStream.ParamTable.Rows];
		MDTable methodTable = tablesStream.MethodTable;
		for (uint num = 1u; num <= methodTable.Rows; num++)
		{
			RidList paramRidList = GetParamRidList(num);
			for (int i = 0; i < paramRidList.Count; i++)
			{
				uint num2 = paramRidList[i] - 1;
				if (array[num2] == 0)
				{
					array[num2] = num;
				}
			}
		}
		Interlocked.CompareExchange(ref paramRidToOwnerRid, array, null);
	}

	public override RidList GetNestedClassRidList(uint typeDefRid)
	{
		if (typeDefRidToNestedClasses == null)
		{
			InitializeNestedClassesDictionary();
		}
		if (typeDefRidToNestedClasses.TryGetValue(typeDefRid, out var value))
		{
			return RidList.Create(value);
		}
		return RidList.Empty;
	}

	private void InitializeNestedClassesDictionary()
	{
		MDTable nestedClassTable = tablesStream.NestedClassTable;
		MDTable typeDefTable = tablesStream.TypeDefTable;
		Dictionary<uint, bool> dictionary = null;
		RidList typeDefRidList = GetTypeDefRidList();
		if (typeDefRidList.Count != (int)typeDefTable.Rows)
		{
			dictionary = new Dictionary<uint, bool>(typeDefRidList.Count);
			for (int i = 0; i < typeDefRidList.Count; i++)
			{
				dictionary[typeDefRidList[i]] = true;
			}
		}
		Dictionary<uint, bool> dictionary2 = new Dictionary<uint, bool>((int)nestedClassTable.Rows);
		List<uint> list = new List<uint>((int)nestedClassTable.Rows);
		for (uint num = 1u; num <= nestedClassTable.Rows; num++)
		{
			if ((dictionary == null || dictionary.ContainsKey(num)) && tablesStream.TryReadNestedClassRow(num, out var row) && typeDefTable.IsValidRID(row.NestedClass) && typeDefTable.IsValidRID(row.EnclosingClass) && !dictionary2.ContainsKey(row.NestedClass))
			{
				dictionary2[row.NestedClass] = true;
				list.Add(row.NestedClass);
			}
		}
		Dictionary<uint, List<uint>> dictionary3 = new Dictionary<uint, List<uint>>();
		int count = list.Count;
		for (int j = 0; j < count; j++)
		{
			uint num2 = list[j];
			if (tablesStream.TryReadNestedClassRow(GetNestedClassRid(num2), out var row2))
			{
				if (!dictionary3.TryGetValue(row2.EnclosingClass, out var value))
				{
					value = (dictionary3[row2.EnclosingClass] = new List<uint>());
				}
				value.Add(num2);
			}
		}
		List<uint> list3 = new List<uint>((int)(typeDefTable.Rows - dictionary2.Count));
		for (uint num3 = 1u; num3 <= typeDefTable.Rows; num3++)
		{
			if ((dictionary == null || dictionary.ContainsKey(num3)) && !dictionary2.ContainsKey(num3))
			{
				list3.Add(num3);
			}
		}
		Interlocked.CompareExchange(ref nonNestedTypes, new StrongBox<RidList>(RidList.Create(list3)), null);
		Interlocked.CompareExchange(ref typeDefRidToNestedClasses, dictionary3, null);
	}

	public override RidList GetNonNestedClassRidList()
	{
		if (typeDefRidToNestedClasses == null)
		{
			InitializeNestedClassesDictionary();
		}
		return nonNestedTypes.Value;
	}

	public override RidList GetLocalScopeRidList(uint methodRid)
	{
		return FindAllRows(tablesStream.LocalScopeTable, 0, methodRid);
	}

	public override uint GetStateMachineMethodRid(uint methodRid)
	{
		RidList ridList = FindAllRows(tablesStream.StateMachineMethodTable, 0, methodRid);
		if (ridList.Count != 0)
		{
			return ridList[0];
		}
		return 0u;
	}

	public override RidList GetCustomDebugInformationRidList(Table table, uint rid)
	{
		if (!CodedToken.HasCustomDebugInformation.Encode(new MDToken(table, rid), out var codedToken))
		{
			return RidList.Empty;
		}
		return FindAllRows(tablesStream.CustomDebugInformationTable, 0, codedToken);
	}

	public override void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposing)
		{
			return;
		}
		peImage?.Dispose();
		stringsStream?.Dispose();
		usStream?.Dispose();
		blobStream?.Dispose();
		guidStream?.Dispose();
		tablesStream?.Dispose();
		IList<DotNetStream> list = allStreams;
		if (list != null)
		{
			foreach (DotNetStream item in list)
			{
				item?.Dispose();
			}
		}
		mdReaderFactoryToDisposeLater?.Dispose();
		peImage = null;
		cor20Header = null;
		mdHeader = null;
		stringsStream = null;
		usStream = null;
		blobStream = null;
		guidStream = null;
		tablesStream = null;
		allStreams = null;
		fieldRidToTypeDefRid = null;
		methodRidToTypeDefRid = null;
		typeDefRidToNestedClasses = null;
		mdReaderFactoryToDisposeLater = null;
	}
}
