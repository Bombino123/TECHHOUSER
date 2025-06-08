using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using dnlib.DotNet.MD;

namespace dnlib.DotNet.Writer;

internal sealed class PreserveTokensMetadata : Metadata
{
	[DebuggerDisplay("{Rid} -> {NewRid} {Def}")]
	private sealed class MemberDefInfo<T> where T : IMDTokenProvider
	{
		public readonly T Def;

		public uint Rid;

		public uint NewRid;

		public MemberDefInfo(T def, uint rid)
		{
			Def = def;
			Rid = rid;
			NewRid = rid;
		}
	}

	[DebuggerDisplay("Count = {Count}")]
	private sealed class MemberDefDict<T> where T : IMDTokenProvider
	{
		private readonly Type defMDType;

		private uint userRid = 16777216u;

		private uint newRid = 1u;

		private int numDefMDs;

		private int numDefUsers;

		private int tableSize;

		private bool wasSorted;

		private readonly bool preserveRids;

		private readonly bool enableRidToInfo;

		private readonly Dictionary<T, MemberDefInfo<T>> defToInfo = new Dictionary<T, MemberDefInfo<T>>();

		private Dictionary<uint, MemberDefInfo<T>> ridToInfo;

		private readonly List<MemberDefInfo<T>> defs = new List<MemberDefInfo<T>>();

		private List<MemberDefInfo<T>> sortedDefs;

		private readonly Dictionary<T, int> collectionPositions = new Dictionary<T, int>();

		public int Count => defs.Count;

		public int TableSize => tableSize;

		public bool NeedPtrTable
		{
			get
			{
				if (preserveRids)
				{
					return !wasSorted;
				}
				return false;
			}
		}

		public MemberDefDict(Type defMDType, bool preserveRids)
			: this(defMDType, preserveRids, enableRidToInfo: false)
		{
		}

		public MemberDefDict(Type defMDType, bool preserveRids, bool enableRidToInfo)
		{
			this.defMDType = defMDType;
			this.preserveRids = preserveRids;
			this.enableRidToInfo = enableRidToInfo;
		}

		public uint Rid(T def)
		{
			return defToInfo[def].Rid;
		}

		public bool TryGetRid(T def, out uint rid)
		{
			if (def == null || !defToInfo.TryGetValue(def, out var value))
			{
				rid = 0u;
				return false;
			}
			rid = value.Rid;
			return true;
		}

		public void Sort(Comparison<MemberDefInfo<T>> comparer)
		{
			if (!preserveRids)
			{
				sortedDefs = defs;
				return;
			}
			sortedDefs = new List<MemberDefInfo<T>>(defs);
			sortedDefs.Sort(comparer);
			wasSorted = true;
			for (int i = 0; i < sortedDefs.Count; i++)
			{
				MemberDefInfo<T> memberDefInfo = sortedDefs[i];
				uint num = (memberDefInfo.NewRid = (uint)(i + 1));
				if (memberDefInfo.Rid != num)
				{
					wasSorted = false;
				}
			}
		}

		public MemberDefInfo<T> Get(int i)
		{
			return defs[i];
		}

		public MemberDefInfo<T> GetSorted(int i)
		{
			return sortedDefs[i];
		}

		public MemberDefInfo<T> GetByRid(uint rid)
		{
			ridToInfo.TryGetValue(rid, out var value);
			return value;
		}

		public void Add(T def, int collPos)
		{
			uint rid;
			if ((object)def.GetType() == defMDType)
			{
				numDefMDs++;
				rid = (preserveRids ? def.Rid : newRid++);
			}
			else
			{
				numDefUsers++;
				rid = (preserveRids ? userRid++ : newRid++);
			}
			MemberDefInfo<T> memberDefInfo = new MemberDefInfo<T>(def, rid);
			defToInfo[def] = memberDefInfo;
			defs.Add(memberDefInfo);
			collectionPositions.Add(def, collPos);
		}

		public void SortDefs()
		{
			if (preserveRids)
			{
				defs.Sort((MemberDefInfo<T> a, MemberDefInfo<T> b) => a.Rid.CompareTo(b.Rid));
				uint num = ((numDefMDs == 0) ? 1u : (defs[numDefMDs - 1].Rid + 1));
				for (int i = numDefMDs; i < defs.Count; i++)
				{
					defs[i].Rid = num++;
				}
				tableSize = (int)(num - 1);
			}
			else
			{
				tableSize = defs.Count;
			}
			if (enableRidToInfo)
			{
				ridToInfo = new Dictionary<uint, MemberDefInfo<T>>(defs.Count);
				foreach (MemberDefInfo<T> def in defs)
				{
					ridToInfo.Add(def.Rid, def);
				}
			}
			if ((uint)tableSize > 16777215u)
			{
				throw new ModuleWriterException("Table is too big");
			}
		}

		public int GetCollectionPosition(T def)
		{
			return collectionPositions[def];
		}
	}

	private readonly ModuleDefMD mod;

	private readonly Rows<TypeRef> typeRefInfos = new Rows<TypeRef>();

	private readonly Dictionary<TypeDef, uint> typeToRid = new Dictionary<TypeDef, uint>();

	private MemberDefDict<FieldDef> fieldDefInfos;

	private MemberDefDict<MethodDef> methodDefInfos;

	private MemberDefDict<ParamDef> paramDefInfos;

	private readonly Rows<MemberRef> memberRefInfos = new Rows<MemberRef>();

	private readonly Rows<StandAloneSig> standAloneSigInfos = new Rows<StandAloneSig>();

	private MemberDefDict<EventDef> eventDefInfos;

	private MemberDefDict<PropertyDef> propertyDefInfos;

	private readonly Rows<TypeSpec> typeSpecInfos = new Rows<TypeSpec>();

	private readonly Rows<MethodSpec> methodSpecInfos = new Rows<MethodSpec>();

	private readonly Dictionary<uint, uint> callConvTokenToSignature = new Dictionary<uint, uint>();

	private bool initdTypeRef;

	private bool initdMemberRef;

	private bool initdStandAloneSig;

	private bool initdTypeSpec;

	private bool initdMethodSpec;

	private uint dummyPtrTableTypeRid;

	protected override int NumberOfMethods => methodDefInfos.Count;

	public PreserveTokensMetadata(ModuleDef module, UniqueChunkList<ByteArrayChunk> constants, MethodBodyChunks methodBodies, NetResources netResources, MetadataOptions options, DebugMetadataKind debugKind, bool isStandaloneDebugMetadata)
		: base(module, constants, methodBodies, netResources, options, debugKind, isStandaloneDebugMetadata)
	{
		mod = module as ModuleDefMD;
		if (mod == null)
		{
			throw new ModuleWriterException("Not a ModuleDefMD");
		}
	}

	public override uint GetRid(TypeRef tr)
	{
		typeRefInfos.TryGetRid(tr, out var rid);
		return rid;
	}

	public override uint GetRid(TypeDef td)
	{
		if (td == null)
		{
			Error("TypeDef is null");
			return 0u;
		}
		if (typeToRid.TryGetValue(td, out var value))
		{
			return value;
		}
		Error("TypeDef '{0}' (0x{1:X8}) is not defined in this module '{2}'. A type was removed that is still referenced by this module.", td, td.MDToken.Raw, module);
		return 0u;
	}

	public override uint GetRid(FieldDef fd)
	{
		if (fieldDefInfos.TryGetRid(fd, out var rid))
		{
			return rid;
		}
		if (fd == null)
		{
			Error("Field is null");
		}
		else
		{
			Error("Field '{0}' (0x{1:X8}) is not defined in this module '{2}'. A field was removed that is still referenced by this module.", fd, fd.MDToken.Raw, module);
		}
		return 0u;
	}

	public override uint GetRid(MethodDef md)
	{
		if (methodDefInfos.TryGetRid(md, out var rid))
		{
			return rid;
		}
		if (md == null)
		{
			Error("Method is null");
		}
		else
		{
			Error("Method '{0}' (0x{1:X8}) is not defined in this module '{2}'. A method was removed that is still referenced by this module.", md, md.MDToken.Raw, module);
		}
		return 0u;
	}

	public override uint GetRid(ParamDef pd)
	{
		if (paramDefInfos.TryGetRid(pd, out var rid))
		{
			return rid;
		}
		if (pd == null)
		{
			Error("Param is null");
		}
		else
		{
			Error("Param '{0}' (0x{1:X8}) is not defined in this module '{2}'. A parameter was removed that is still referenced by this module.", pd, pd.MDToken.Raw, module);
		}
		return 0u;
	}

	public override uint GetRid(MemberRef mr)
	{
		memberRefInfos.TryGetRid(mr, out var rid);
		return rid;
	}

	public override uint GetRid(StandAloneSig sas)
	{
		standAloneSigInfos.TryGetRid(sas, out var rid);
		return rid;
	}

	public override uint GetRid(EventDef ed)
	{
		if (eventDefInfos.TryGetRid(ed, out var rid))
		{
			return rid;
		}
		if (ed == null)
		{
			Error("Event is null");
		}
		else
		{
			Error("Event '{0}' (0x{1:X8}) is not defined in this module '{2}'. An event was removed that is still referenced by this module.", ed, ed.MDToken.Raw, module);
		}
		return 0u;
	}

	public override uint GetRid(PropertyDef pd)
	{
		if (propertyDefInfos.TryGetRid(pd, out var rid))
		{
			return rid;
		}
		if (pd == null)
		{
			Error("Property is null");
		}
		else
		{
			Error("Property '{0}' (0x{1:X8}) is not defined in this module '{2}'. A property was removed that is still referenced by this module.", pd, pd.MDToken.Raw, module);
		}
		return 0u;
	}

	public override uint GetRid(TypeSpec ts)
	{
		typeSpecInfos.TryGetRid(ts, out var rid);
		return rid;
	}

	public override uint GetRid(MethodSpec ms)
	{
		methodSpecInfos.TryGetRid(ms, out var rid);
		return rid;
	}

	protected override void Initialize()
	{
		fieldDefInfos = new MemberDefDict<FieldDef>(typeof(FieldDefMD), base.PreserveFieldRids);
		methodDefInfos = new MemberDefDict<MethodDef>(typeof(MethodDefMD), base.PreserveMethodRids, enableRidToInfo: true);
		paramDefInfos = new MemberDefDict<ParamDef>(typeof(ParamDefMD), base.PreserveParamRids);
		eventDefInfos = new MemberDefDict<EventDef>(typeof(EventDefMD), base.PreserveEventRids);
		propertyDefInfos = new MemberDefDict<PropertyDef>(typeof(PropertyDefMD), base.PreservePropertyRids);
		CreateEmptyTableRows();
	}

	protected override TypeDef[] GetAllTypeDefs()
	{
		if (!base.PreserveTypeDefRids)
		{
			TypeDef[] array = module.GetTypes().ToArray();
			InitializeTypeToRid(array);
			return array;
		}
		Dictionary<TypeDef, uint> typeToIndex = new Dictionary<TypeDef, uint>();
		List<TypeDef> list = new List<TypeDef>();
		uint num = 0u;
		foreach (TypeDef type in module.GetTypes())
		{
			if (type != null)
			{
				list.Add(type);
				uint num2 = num++;
				if ((object)type.GetType() == typeof(TypeDefMD))
				{
					num2 |= 0x80000000u;
				}
				typeToIndex[type] = num2;
			}
		}
		TypeDef globalType = list[0];
		list.Sort(delegate(TypeDef a, TypeDef b)
		{
			if (a == b)
			{
				return 0;
			}
			if (a == globalType)
			{
				return -1;
			}
			if (b == globalType)
			{
				return 1;
			}
			uint num5 = typeToIndex[a];
			uint num6 = typeToIndex[b];
			bool flag = (num5 & 0x80000000u) != 0;
			bool flag2 = (num6 & 0x80000000u) != 0;
			if (flag == flag2)
			{
				if (flag)
				{
					return a.Rid.CompareTo(b.Rid);
				}
				return (num5 & 0xFFFFFFu).CompareTo(num6 & 0xFFFFFFu);
			}
			return (!flag) ? 1 : (-1);
		});
		List<TypeDef> list2 = new List<TypeDef>(list.Count);
		uint num3 = 1u;
		list2.Add(globalType);
		for (int i = 1; i < list.Count; i++)
		{
			TypeDef typeDef = list[i];
			if ((object)typeDef.GetType() != typeof(TypeDefMD))
			{
				while (i < list.Count)
				{
					list2.Add(list[i++]);
				}
				break;
			}
			uint rid = typeDef.Rid;
			int num4 = (int)(rid - num3 - 1);
			if (num4 != 0)
			{
				for (int j = 0; j < num4; j++)
				{
					list2.Add(new TypeDefUser("dummy", Guid.NewGuid().ToString("B"), module.CorLibTypes.Object.TypeDefOrRef));
				}
			}
			list2.Add(typeDef);
			num3 = rid;
		}
		TypeDef[] array2 = list2.ToArray();
		InitializeTypeToRid(array2);
		return array2;
	}

	private void InitializeTypeToRid(TypeDef[] types)
	{
		uint num = 1u;
		foreach (TypeDef typeDef in types)
		{
			if (typeDef != null && !typeToRid.ContainsKey(typeDef))
			{
				typeToRid[typeDef] = num++;
			}
		}
	}

	protected override void AllocateTypeDefRids()
	{
		TypeDef[] array = allTypeDefs;
		foreach (TypeDef key in array)
		{
			uint num = tablesHeap.TypeDefTable.Create(default(RawTypeDefRow));
			if (typeToRid[key] != num)
			{
				throw new ModuleWriterException("Got a different rid than expected");
			}
		}
	}

	private void CreateEmptyTableRows()
	{
		if (base.PreserveTypeRefRids)
		{
			uint rows = mod.TablesStream.TypeRefTable.Rows;
			for (uint num = 0u; num < rows; num++)
			{
				tablesHeap.TypeRefTable.Create(default(RawTypeRefRow));
			}
		}
		if (base.PreserveMemberRefRids)
		{
			uint rows = mod.TablesStream.MemberRefTable.Rows;
			for (uint num2 = 0u; num2 < rows; num2++)
			{
				tablesHeap.MemberRefTable.Create(default(RawMemberRefRow));
			}
		}
		if (base.PreserveStandAloneSigRids)
		{
			uint rows = mod.TablesStream.StandAloneSigTable.Rows;
			for (uint num3 = 0u; num3 < rows; num3++)
			{
				tablesHeap.StandAloneSigTable.Create(default(RawStandAloneSigRow));
			}
		}
		if (base.PreserveTypeSpecRids)
		{
			uint rows = mod.TablesStream.TypeSpecTable.Rows;
			for (uint num4 = 0u; num4 < rows; num4++)
			{
				tablesHeap.TypeSpecTable.Create(default(RawTypeSpecRow));
			}
		}
		if (base.PreserveMethodSpecRids)
		{
			uint rows = mod.TablesStream.MethodSpecTable.Rows;
			for (uint num5 = 0u; num5 < rows; num5++)
			{
				tablesHeap.MethodSpecTable.Create(default(RawMethodSpecRow));
			}
		}
	}

	private void InitializeUninitializedTableRows()
	{
		InitializeTypeRefTableRows();
		InitializeMemberRefTableRows();
		InitializeStandAloneSigTableRows();
		InitializeTypeSpecTableRows();
		InitializeMethodSpecTableRows();
	}

	private void InitializeTypeRefTableRows()
	{
		if (base.PreserveTypeRefRids && !initdTypeRef)
		{
			initdTypeRef = true;
			uint rows = mod.TablesStream.TypeRefTable.Rows;
			for (uint num = 1u; num <= rows; num++)
			{
				AddTypeRef(mod.ResolveTypeRef(num));
			}
			tablesHeap.TypeRefTable.ReAddRows();
		}
	}

	private void InitializeMemberRefTableRows()
	{
		if (!base.PreserveMemberRefRids || initdMemberRef)
		{
			return;
		}
		initdMemberRef = true;
		uint rows = mod.TablesStream.MemberRefTable.Rows;
		for (uint num = 1u; num <= rows; num++)
		{
			if (tablesHeap.MemberRefTable[num].Class == 0)
			{
				AddMemberRef(mod.ResolveMemberRef(num), forceIsOld: true);
			}
		}
		tablesHeap.MemberRefTable.ReAddRows();
	}

	private void InitializeStandAloneSigTableRows()
	{
		if (!base.PreserveStandAloneSigRids || initdStandAloneSig)
		{
			return;
		}
		initdStandAloneSig = true;
		uint rows = mod.TablesStream.StandAloneSigTable.Rows;
		for (uint num = 1u; num <= rows; num++)
		{
			if (tablesHeap.StandAloneSigTable[num].Signature == 0)
			{
				AddStandAloneSig(mod.ResolveStandAloneSig(num), forceIsOld: true);
			}
		}
		tablesHeap.StandAloneSigTable.ReAddRows();
	}

	private void InitializeTypeSpecTableRows()
	{
		if (!base.PreserveTypeSpecRids || initdTypeSpec)
		{
			return;
		}
		initdTypeSpec = true;
		uint rows = mod.TablesStream.TypeSpecTable.Rows;
		for (uint num = 1u; num <= rows; num++)
		{
			if (tablesHeap.TypeSpecTable[num].Signature == 0)
			{
				AddTypeSpec(mod.ResolveTypeSpec(num), forceIsOld: true);
			}
		}
		tablesHeap.TypeSpecTable.ReAddRows();
	}

	private void InitializeMethodSpecTableRows()
	{
		if (!base.PreserveMethodSpecRids || initdMethodSpec)
		{
			return;
		}
		initdMethodSpec = true;
		uint rows = mod.TablesStream.MethodSpecTable.Rows;
		for (uint num = 1u; num <= rows; num++)
		{
			if (tablesHeap.MethodSpecTable[num].Method == 0)
			{
				AddMethodSpec(mod.ResolveMethodSpec(num), forceIsOld: true);
			}
		}
		tablesHeap.MethodSpecTable.ReAddRows();
	}

	protected override void AllocateMemberDefRids()
	{
		FindMemberDefs();
		RaiseProgress(dnlib.DotNet.Writer.MetadataEvent.AllocateMemberDefRids, 0.0);
		for (int i = 1; i <= fieldDefInfos.TableSize; i++)
		{
			if (i != (int)tablesHeap.FieldTable.Create(default(RawFieldRow)))
			{
				throw new ModuleWriterException("Invalid field rid");
			}
		}
		for (int j = 1; j <= methodDefInfos.TableSize; j++)
		{
			if (j != (int)tablesHeap.MethodTable.Create(default(RawMethodRow)))
			{
				throw new ModuleWriterException("Invalid method rid");
			}
		}
		for (int k = 1; k <= paramDefInfos.TableSize; k++)
		{
			if (k != (int)tablesHeap.ParamTable.Create(default(RawParamRow)))
			{
				throw new ModuleWriterException("Invalid param rid");
			}
		}
		for (int l = 1; l <= eventDefInfos.TableSize; l++)
		{
			if (l != (int)tablesHeap.EventTable.Create(default(RawEventRow)))
			{
				throw new ModuleWriterException("Invalid event rid");
			}
		}
		for (int m = 1; m <= propertyDefInfos.TableSize; m++)
		{
			if (m != (int)tablesHeap.PropertyTable.Create(default(RawPropertyRow)))
			{
				throw new ModuleWriterException("Invalid property rid");
			}
		}
		SortFields();
		SortMethods();
		SortParameters();
		SortEvents();
		SortProperties();
		RaiseProgress(dnlib.DotNet.Writer.MetadataEvent.AllocateMemberDefRids, 0.2);
		if (fieldDefInfos.NeedPtrTable)
		{
			for (int n = 0; n < fieldDefInfos.Count; n++)
			{
				MemberDefInfo<FieldDef> sorted = fieldDefInfos.GetSorted(n);
				if (n + 1 != (int)tablesHeap.FieldPtrTable.Add(new RawFieldPtrRow(sorted.Rid)))
				{
					throw new ModuleWriterException("Invalid field ptr rid");
				}
			}
			ReUseDeletedFieldRows();
		}
		if (methodDefInfos.NeedPtrTable)
		{
			for (int num = 0; num < methodDefInfos.Count; num++)
			{
				MemberDefInfo<MethodDef> sorted2 = methodDefInfos.GetSorted(num);
				if (num + 1 != (int)tablesHeap.MethodPtrTable.Add(new RawMethodPtrRow(sorted2.Rid)))
				{
					throw new ModuleWriterException("Invalid method ptr rid");
				}
			}
			ReUseDeletedMethodRows();
		}
		if (paramDefInfos.NeedPtrTable)
		{
			for (int num2 = 0; num2 < paramDefInfos.Count; num2++)
			{
				MemberDefInfo<ParamDef> sorted3 = paramDefInfos.GetSorted(num2);
				if (num2 + 1 != (int)tablesHeap.ParamPtrTable.Add(new RawParamPtrRow(sorted3.Rid)))
				{
					throw new ModuleWriterException("Invalid param ptr rid");
				}
			}
			ReUseDeletedParamRows();
		}
		if (eventDefInfos.NeedPtrTable)
		{
			for (int num3 = 0; num3 < eventDefInfos.Count; num3++)
			{
				MemberDefInfo<EventDef> sorted4 = eventDefInfos.GetSorted(num3);
				if (num3 + 1 != (int)tablesHeap.EventPtrTable.Add(new RawEventPtrRow(sorted4.Rid)))
				{
					throw new ModuleWriterException("Invalid event ptr rid");
				}
			}
		}
		if (propertyDefInfos.NeedPtrTable)
		{
			for (int num4 = 0; num4 < propertyDefInfos.Count; num4++)
			{
				MemberDefInfo<PropertyDef> sorted5 = propertyDefInfos.GetSorted(num4);
				if (num4 + 1 != (int)tablesHeap.PropertyPtrTable.Add(new RawPropertyPtrRow(sorted5.Rid)))
				{
					throw new ModuleWriterException("Invalid property ptr rid");
				}
			}
		}
		RaiseProgress(dnlib.DotNet.Writer.MetadataEvent.AllocateMemberDefRids, 0.4);
		InitializeMethodAndFieldList();
		InitializeParamList();
		InitializeEventMap();
		InitializePropertyMap();
		RaiseProgress(dnlib.DotNet.Writer.MetadataEvent.AllocateMemberDefRids, 0.6);
		if (eventDefInfos.NeedPtrTable)
		{
			ReUseDeletedEventRows();
		}
		if (propertyDefInfos.NeedPtrTable)
		{
			ReUseDeletedPropertyRows();
		}
		RaiseProgress(dnlib.DotNet.Writer.MetadataEvent.AllocateMemberDefRids, 0.8);
		InitializeTypeRefTableRows();
		InitializeTypeSpecTableRows();
		InitializeMemberRefTableRows();
		InitializeMethodSpecTableRows();
	}

	private void ReUseDeletedFieldRows()
	{
		if (tablesHeap.FieldPtrTable.IsEmpty || fieldDefInfos.TableSize == tablesHeap.FieldPtrTable.Rows)
		{
			return;
		}
		bool[] array = new bool[fieldDefInfos.TableSize];
		for (int i = 0; i < fieldDefInfos.Count; i++)
		{
			array[fieldDefInfos.Get(i).Rid - 1] = true;
		}
		CreateDummyPtrTableType();
		uint signature = GetSignature(new FieldSig(module.CorLibTypes.Byte));
		for (int j = 0; j < array.Length; j++)
		{
			if (!array[j])
			{
				uint num = (uint)(j + 1);
				RawFieldRow value = new RawFieldRow(22, stringsHeap.Add($"f{num:X6}"), signature);
				tablesHeap.FieldTable[num] = value;
				tablesHeap.FieldPtrTable.Create(new RawFieldPtrRow(num));
			}
		}
		if (fieldDefInfos.TableSize == tablesHeap.FieldPtrTable.Rows)
		{
			return;
		}
		throw new ModuleWriterException("Didn't create all dummy fields");
	}

	private void ReUseDeletedMethodRows()
	{
		if (tablesHeap.MethodPtrTable.IsEmpty || methodDefInfos.TableSize == tablesHeap.MethodPtrTable.Rows)
		{
			return;
		}
		bool[] array = new bool[methodDefInfos.TableSize];
		for (int i = 0; i < methodDefInfos.Count; i++)
		{
			array[methodDefInfos.Get(i).Rid - 1] = true;
		}
		CreateDummyPtrTableType();
		uint signature = GetSignature(MethodSig.CreateInstance(module.CorLibTypes.Void));
		for (int j = 0; j < array.Length; j++)
		{
			if (!array[j])
			{
				uint num = (uint)(j + 1);
				RawMethodRow value = new RawMethodRow(0u, 0, 1478, stringsHeap.Add($"m{num:X6}"), signature, (uint)paramDefInfos.Count);
				tablesHeap.MethodTable[num] = value;
				tablesHeap.MethodPtrTable.Create(new RawMethodPtrRow(num));
			}
		}
		if (methodDefInfos.TableSize == tablesHeap.MethodPtrTable.Rows)
		{
			return;
		}
		throw new ModuleWriterException("Didn't create all dummy methods");
	}

	private void ReUseDeletedParamRows()
	{
		if (tablesHeap.ParamPtrTable.IsEmpty || paramDefInfos.TableSize == tablesHeap.ParamPtrTable.Rows)
		{
			return;
		}
		bool[] array = new bool[paramDefInfos.TableSize];
		for (int i = 0; i < paramDefInfos.Count; i++)
		{
			array[paramDefInfos.Get(i).Rid - 1] = true;
		}
		CreateDummyPtrTableType();
		uint signature = GetSignature(MethodSig.CreateInstance(module.CorLibTypes.Void));
		for (int j = 0; j < array.Length; j++)
		{
			if (!array[j])
			{
				uint num = (uint)(j + 1);
				RawParamRow value = new RawParamRow(0, 0, stringsHeap.Add($"p{num:X6}"));
				tablesHeap.ParamTable[num] = value;
				uint paramList = tablesHeap.ParamPtrTable.Create(new RawParamPtrRow(num));
				RawMethodRow row = new RawMethodRow(0u, 0, 1478, stringsHeap.Add($"mp{num:X6}"), signature, paramList);
				uint method = tablesHeap.MethodTable.Create(row);
				if (tablesHeap.MethodPtrTable.Rows > 0)
				{
					tablesHeap.MethodPtrTable.Create(new RawMethodPtrRow(method));
				}
			}
		}
		if (paramDefInfos.TableSize == tablesHeap.ParamPtrTable.Rows)
		{
			return;
		}
		throw new ModuleWriterException("Didn't create all dummy params");
	}

	private void ReUseDeletedEventRows()
	{
		if (tablesHeap.EventPtrTable.IsEmpty || eventDefInfos.TableSize == tablesHeap.EventPtrTable.Rows)
		{
			return;
		}
		bool[] array = new bool[eventDefInfos.TableSize];
		for (int i = 0; i < eventDefInfos.Count; i++)
		{
			array[eventDefInfos.Get(i).Rid - 1] = true;
		}
		uint parent = CreateDummyPtrTableType();
		tablesHeap.EventMapTable.Create(new RawEventMapRow(parent, (uint)(tablesHeap.EventPtrTable.Rows + 1)));
		uint eventType = AddTypeDefOrRef(module.CorLibTypes.Object.TypeDefOrRef);
		for (int j = 0; j < array.Length; j++)
		{
			if (!array[j])
			{
				uint num = (uint)(j + 1);
				RawEventRow value = new RawEventRow(0, stringsHeap.Add($"E{num:X6}"), eventType);
				tablesHeap.EventTable[num] = value;
				tablesHeap.EventPtrTable.Create(new RawEventPtrRow(num));
			}
		}
		if (eventDefInfos.TableSize == tablesHeap.EventPtrTable.Rows)
		{
			return;
		}
		throw new ModuleWriterException("Didn't create all dummy events");
	}

	private void ReUseDeletedPropertyRows()
	{
		if (tablesHeap.PropertyPtrTable.IsEmpty || propertyDefInfos.TableSize == tablesHeap.PropertyPtrTable.Rows)
		{
			return;
		}
		bool[] array = new bool[propertyDefInfos.TableSize];
		for (int i = 0; i < propertyDefInfos.Count; i++)
		{
			array[propertyDefInfos.Get(i).Rid - 1] = true;
		}
		uint parent = CreateDummyPtrTableType();
		tablesHeap.PropertyMapTable.Create(new RawPropertyMapRow(parent, (uint)(tablesHeap.PropertyPtrTable.Rows + 1)));
		uint signature = GetSignature(PropertySig.CreateStatic(module.CorLibTypes.Object));
		for (int j = 0; j < array.Length; j++)
		{
			if (!array[j])
			{
				uint num = (uint)(j + 1);
				RawPropertyRow value = new RawPropertyRow(0, stringsHeap.Add($"P{num:X6}"), signature);
				tablesHeap.PropertyTable[num] = value;
				tablesHeap.PropertyPtrTable.Create(new RawPropertyPtrRow(num));
			}
		}
		if (propertyDefInfos.TableSize == tablesHeap.PropertyPtrTable.Rows)
		{
			return;
		}
		throw new ModuleWriterException("Didn't create all dummy properties");
	}

	private uint CreateDummyPtrTableType()
	{
		if (dummyPtrTableTypeRid != 0)
		{
			return dummyPtrTableTypeRid;
		}
		TypeAttributes flags = TypeAttributes.Abstract;
		int num = (fieldDefInfos.NeedPtrTable ? fieldDefInfos.Count : fieldDefInfos.TableSize);
		int num2 = (methodDefInfos.NeedPtrTable ? methodDefInfos.Count : methodDefInfos.TableSize);
		RawTypeDefRow row = new RawTypeDefRow((uint)flags, stringsHeap.Add(Guid.NewGuid().ToString("B")), stringsHeap.Add("dummy_ptr"), AddTypeDefOrRef(module.CorLibTypes.Object.TypeDefOrRef), (uint)(num + 1), (uint)(num2 + 1));
		dummyPtrTableTypeRid = tablesHeap.TypeDefTable.Create(row);
		if (dummyPtrTableTypeRid == 1)
		{
			throw new ModuleWriterException("Dummy ptr type is the first type");
		}
		return dummyPtrTableTypeRid;
	}

	private void FindMemberDefs()
	{
		Dictionary<object, bool> dictionary = new Dictionary<object, bool>();
		TypeDef[] array = allTypeDefs;
		foreach (TypeDef typeDef in array)
		{
			if (typeDef == null)
			{
				continue;
			}
			int num = 0;
			IList<FieldDef> fields = typeDef.Fields;
			int count = fields.Count;
			for (int j = 0; j < count; j++)
			{
				FieldDef fieldDef = fields[j];
				if (fieldDef != null)
				{
					fieldDefInfos.Add(fieldDef, num++);
				}
			}
			num = 0;
			IList<MethodDef> methods = typeDef.Methods;
			count = methods.Count;
			for (int k = 0; k < count; k++)
			{
				MethodDef methodDef = methods[k];
				if (methodDef != null)
				{
					methodDefInfos.Add(methodDef, num++);
				}
			}
			num = 0;
			IList<EventDef> events = typeDef.Events;
			count = events.Count;
			for (int l = 0; l < count; l++)
			{
				EventDef eventDef = events[l];
				if (eventDef != null && !dictionary.ContainsKey(eventDef))
				{
					dictionary[eventDef] = true;
					eventDefInfos.Add(eventDef, num++);
				}
			}
			num = 0;
			IList<PropertyDef> properties = typeDef.Properties;
			count = properties.Count;
			for (int m = 0; m < count; m++)
			{
				PropertyDef propertyDef = properties[m];
				if (propertyDef != null && !dictionary.ContainsKey(propertyDef))
				{
					dictionary[propertyDef] = true;
					propertyDefInfos.Add(propertyDef, num++);
				}
			}
		}
		fieldDefInfos.SortDefs();
		methodDefInfos.SortDefs();
		eventDefInfos.SortDefs();
		propertyDefInfos.SortDefs();
		for (int n = 0; n < methodDefInfos.Count; n++)
		{
			MethodDef def = methodDefInfos.Get(n).Def;
			int num = 0;
			foreach (ParamDef item in Metadata.Sort(def.ParamDefs))
			{
				if (item != null)
				{
					paramDefInfos.Add(item, num++);
				}
			}
		}
		paramDefInfos.SortDefs();
	}

	private void SortFields()
	{
		fieldDefInfos.Sort(delegate(MemberDefInfo<FieldDef> a, MemberDefInfo<FieldDef> b)
		{
			uint num = ((a.Def.DeclaringType != null) ? typeToRid[a.Def.DeclaringType] : 0u);
			uint num2 = ((b.Def.DeclaringType != null) ? typeToRid[b.Def.DeclaringType] : 0u);
			if (num == 0 || num2 == 0)
			{
				return a.Rid.CompareTo(b.Rid);
			}
			return (num != num2) ? num.CompareTo(num2) : fieldDefInfos.GetCollectionPosition(a.Def).CompareTo(fieldDefInfos.GetCollectionPosition(b.Def));
		});
	}

	private void SortMethods()
	{
		methodDefInfos.Sort(delegate(MemberDefInfo<MethodDef> a, MemberDefInfo<MethodDef> b)
		{
			uint num = ((a.Def.DeclaringType != null) ? typeToRid[a.Def.DeclaringType] : 0u);
			uint num2 = ((b.Def.DeclaringType != null) ? typeToRid[b.Def.DeclaringType] : 0u);
			if (num == 0 || num2 == 0)
			{
				return a.Rid.CompareTo(b.Rid);
			}
			return (num != num2) ? num.CompareTo(num2) : methodDefInfos.GetCollectionPosition(a.Def).CompareTo(methodDefInfos.GetCollectionPosition(b.Def));
		});
	}

	private void SortParameters()
	{
		paramDefInfos.Sort(delegate(MemberDefInfo<ParamDef> a, MemberDefInfo<ParamDef> b)
		{
			uint num = ((a.Def.DeclaringMethod != null) ? methodDefInfos.Rid(a.Def.DeclaringMethod) : 0u);
			uint num2 = ((b.Def.DeclaringMethod != null) ? methodDefInfos.Rid(b.Def.DeclaringMethod) : 0u);
			if (num == 0 || num2 == 0)
			{
				return a.Rid.CompareTo(b.Rid);
			}
			return (num != num2) ? num.CompareTo(num2) : paramDefInfos.GetCollectionPosition(a.Def).CompareTo(paramDefInfos.GetCollectionPosition(b.Def));
		});
	}

	private void SortEvents()
	{
		eventDefInfos.Sort(delegate(MemberDefInfo<EventDef> a, MemberDefInfo<EventDef> b)
		{
			uint num = ((a.Def.DeclaringType != null) ? typeToRid[a.Def.DeclaringType] : 0u);
			uint num2 = ((b.Def.DeclaringType != null) ? typeToRid[b.Def.DeclaringType] : 0u);
			if (num == 0 || num2 == 0)
			{
				return a.Rid.CompareTo(b.Rid);
			}
			return (num != num2) ? num.CompareTo(num2) : eventDefInfos.GetCollectionPosition(a.Def).CompareTo(eventDefInfos.GetCollectionPosition(b.Def));
		});
	}

	private void SortProperties()
	{
		propertyDefInfos.Sort(delegate(MemberDefInfo<PropertyDef> a, MemberDefInfo<PropertyDef> b)
		{
			uint num = ((a.Def.DeclaringType != null) ? typeToRid[a.Def.DeclaringType] : 0u);
			uint num2 = ((b.Def.DeclaringType != null) ? typeToRid[b.Def.DeclaringType] : 0u);
			if (num == 0 || num2 == 0)
			{
				return a.Rid.CompareTo(b.Rid);
			}
			return (num != num2) ? num.CompareTo(num2) : propertyDefInfos.GetCollectionPosition(a.Def).CompareTo(propertyDefInfos.GetCollectionPosition(b.Def));
		});
	}

	private void InitializeMethodAndFieldList()
	{
		uint num = 1u;
		uint num2 = 1u;
		TypeDef[] array = allTypeDefs;
		foreach (TypeDef typeDef in array)
		{
			uint rid = typeToRid[typeDef];
			RawTypeDefRow rawTypeDefRow = tablesHeap.TypeDefTable[rid];
			rawTypeDefRow = new RawTypeDefRow(rawTypeDefRow.Flags, rawTypeDefRow.Name, rawTypeDefRow.Namespace, rawTypeDefRow.Extends, num, num2);
			tablesHeap.TypeDefTable[rid] = rawTypeDefRow;
			num += (uint)typeDef.Fields.Count;
			num2 += (uint)typeDef.Methods.Count;
		}
	}

	private void InitializeParamList()
	{
		uint num = 1u;
		for (uint num2 = 1u; num2 <= methodDefInfos.TableSize; num2++)
		{
			MemberDefInfo<MethodDef> byRid = methodDefInfos.GetByRid(num2);
			RawMethodRow rawMethodRow = tablesHeap.MethodTable[num2];
			rawMethodRow = new RawMethodRow(rawMethodRow.RVA, rawMethodRow.ImplFlags, rawMethodRow.Flags, rawMethodRow.Name, rawMethodRow.Signature, num);
			tablesHeap.MethodTable[num2] = rawMethodRow;
			if (byRid != null)
			{
				num += (uint)byRid.Def.ParamDefs.Count;
			}
		}
	}

	private void InitializeEventMap()
	{
		if (!tablesHeap.EventMapTable.IsEmpty)
		{
			throw new ModuleWriterException("EventMap table isn't empty");
		}
		TypeDef typeDef = null;
		for (int i = 0; i < eventDefInfos.Count; i++)
		{
			MemberDefInfo<EventDef> sorted = eventDefInfos.GetSorted(i);
			if (typeDef != sorted.Def.DeclaringType)
			{
				typeDef = sorted.Def.DeclaringType;
				RawEventMapRow row = new RawEventMapRow(typeToRid[typeDef], sorted.NewRid);
				uint rid = tablesHeap.EventMapTable.Create(row);
				eventMapInfos.Add(typeDef, rid);
			}
		}
	}

	private void InitializePropertyMap()
	{
		if (!tablesHeap.PropertyMapTable.IsEmpty)
		{
			throw new ModuleWriterException("PropertyMap table isn't empty");
		}
		TypeDef typeDef = null;
		for (int i = 0; i < propertyDefInfos.Count; i++)
		{
			MemberDefInfo<PropertyDef> sorted = propertyDefInfos.GetSorted(i);
			if (typeDef != sorted.Def.DeclaringType)
			{
				typeDef = sorted.Def.DeclaringType;
				RawPropertyMapRow row = new RawPropertyMapRow(typeToRid[typeDef], sorted.NewRid);
				uint rid = tablesHeap.PropertyMapTable.Create(row);
				propertyMapInfos.Add(typeDef, rid);
			}
		}
	}

	protected override uint AddTypeRef(TypeRef tr)
	{
		if (tr == null)
		{
			Error("TypeRef is null");
			return 0u;
		}
		if (typeRefInfos.TryGetRid(tr, out var rid))
		{
			if (rid == 0)
			{
				Error("TypeRef 0x{0:X8} has an infinite ResolutionScope loop.", tr.MDToken.Raw);
			}
			return rid;
		}
		typeRefInfos.Add(tr, 0u);
		bool num = base.PreserveTypeRefRids && mod.ResolveTypeRef(tr.Rid) == tr;
		RawTypeRefRow rawTypeRefRow = new RawTypeRefRow(AddResolutionScope(tr.ResolutionScope), stringsHeap.Add(tr.Name), stringsHeap.Add(tr.Namespace));
		if (num)
		{
			rid = tr.Rid;
			tablesHeap.TypeRefTable[tr.Rid] = rawTypeRefRow;
		}
		else
		{
			rid = tablesHeap.TypeRefTable.Add(rawTypeRefRow);
		}
		typeRefInfos.SetRid(tr, rid);
		AddCustomAttributes(Table.TypeRef, rid, tr);
		AddCustomDebugInformationList(Table.TypeRef, rid, tr);
		return rid;
	}

	protected override uint AddTypeSpec(TypeSpec ts)
	{
		return AddTypeSpec(ts, forceIsOld: false);
	}

	private uint AddTypeSpec(TypeSpec ts, bool forceIsOld)
	{
		if (ts == null)
		{
			Error("TypeSpec is null");
			return 0u;
		}
		if (typeSpecInfos.TryGetRid(ts, out var rid))
		{
			if (rid == 0)
			{
				Error("TypeSpec 0x{0:X8} has an infinite TypeSig loop.", ts.MDToken.Raw);
			}
			return rid;
		}
		typeSpecInfos.Add(ts, 0u);
		bool num = forceIsOld || (base.PreserveTypeSpecRids && mod.ResolveTypeSpec(ts.Rid) == ts);
		RawTypeSpecRow rawTypeSpecRow = new RawTypeSpecRow(GetSignature(ts.TypeSig, ts.ExtraData));
		if (num)
		{
			rid = ts.Rid;
			tablesHeap.TypeSpecTable[ts.Rid] = rawTypeSpecRow;
		}
		else
		{
			rid = tablesHeap.TypeSpecTable.Add(rawTypeSpecRow);
		}
		typeSpecInfos.SetRid(ts, rid);
		AddCustomAttributes(Table.TypeSpec, rid, ts);
		AddCustomDebugInformationList(Table.TypeSpec, rid, ts);
		return rid;
	}

	protected override uint AddMemberRef(MemberRef mr)
	{
		return AddMemberRef(mr, forceIsOld: false);
	}

	private uint AddMemberRef(MemberRef mr, bool forceIsOld)
	{
		if (mr == null)
		{
			Error("MemberRef is null");
			return 0u;
		}
		if (memberRefInfos.TryGetRid(mr, out var rid))
		{
			return rid;
		}
		bool num = forceIsOld || (base.PreserveMemberRefRids && mod.ResolveMemberRef(mr.Rid) == mr);
		RawMemberRefRow rawMemberRefRow = new RawMemberRefRow(AddMemberRefParent(mr.Class), stringsHeap.Add(mr.Name), GetSignature(mr.Signature));
		if (num)
		{
			rid = mr.Rid;
			tablesHeap.MemberRefTable[mr.Rid] = rawMemberRefRow;
		}
		else
		{
			rid = tablesHeap.MemberRefTable.Add(rawMemberRefRow);
		}
		memberRefInfos.Add(mr, rid);
		AddCustomAttributes(Table.MemberRef, rid, mr);
		AddCustomDebugInformationList(Table.MemberRef, rid, mr);
		return rid;
	}

	protected override uint AddStandAloneSig(StandAloneSig sas)
	{
		return AddStandAloneSig(sas, forceIsOld: false);
	}

	private uint AddStandAloneSig(StandAloneSig sas, bool forceIsOld)
	{
		if (sas == null)
		{
			Error("StandAloneSig is null");
			return 0u;
		}
		if (standAloneSigInfos.TryGetRid(sas, out var rid))
		{
			return rid;
		}
		bool num = forceIsOld || (base.PreserveStandAloneSigRids && mod.ResolveStandAloneSig(sas.Rid) == sas);
		RawStandAloneSigRow rawStandAloneSigRow = new RawStandAloneSigRow(GetSignature(sas.Signature));
		if (num)
		{
			rid = sas.Rid;
			tablesHeap.StandAloneSigTable[sas.Rid] = rawStandAloneSigRow;
		}
		else
		{
			rid = tablesHeap.StandAloneSigTable.Add(rawStandAloneSigRow);
		}
		standAloneSigInfos.Add(sas, rid);
		AddCustomAttributes(Table.StandAloneSig, rid, sas);
		AddCustomDebugInformationList(Table.StandAloneSig, rid, sas);
		return rid;
	}

	public override MDToken GetToken(IList<TypeSig> locals, uint origToken)
	{
		if (!base.PreserveStandAloneSigRids || !IsValidStandAloneSigToken(origToken))
		{
			return base.GetToken(locals, origToken);
		}
		uint num = AddStandAloneSig(new LocalSig(locals, dummy: false), origToken);
		if (num == 0)
		{
			return base.GetToken(locals, origToken);
		}
		return new MDToken(Table.StandAloneSig, num);
	}

	protected override uint AddStandAloneSig(MethodSig methodSig, uint origToken)
	{
		if (!base.PreserveStandAloneSigRids || !IsValidStandAloneSigToken(origToken))
		{
			return base.AddStandAloneSig(methodSig, origToken);
		}
		uint num = AddStandAloneSig(methodSig, origToken);
		if (num == 0)
		{
			return base.AddStandAloneSig(methodSig, origToken);
		}
		return num;
	}

	protected override uint AddStandAloneSig(FieldSig fieldSig, uint origToken)
	{
		if (!base.PreserveStandAloneSigRids || !IsValidStandAloneSigToken(origToken))
		{
			return base.AddStandAloneSig(fieldSig, origToken);
		}
		uint num = AddStandAloneSig(fieldSig, origToken);
		if (num == 0)
		{
			return base.AddStandAloneSig(fieldSig, origToken);
		}
		return num;
	}

	private uint AddStandAloneSig(CallingConventionSig callConvSig, uint origToken)
	{
		uint signature = GetSignature(callConvSig);
		if (callConvTokenToSignature.TryGetValue(origToken, out var value))
		{
			if (signature == value)
			{
				return MDToken.ToRID(origToken);
			}
			Warning("Could not preserve StandAloneSig token 0x{0:X8}", origToken);
			return 0u;
		}
		uint rid = MDToken.ToRID(origToken);
		StandAloneSig standAloneSig = mod.ResolveStandAloneSig(rid);
		if (standAloneSigInfos.Exists(standAloneSig))
		{
			Warning("StandAloneSig 0x{0:X8} already exists", origToken);
			return 0u;
		}
		CallingConventionSig signature2 = standAloneSig.Signature;
		try
		{
			standAloneSig.Signature = callConvSig;
			AddStandAloneSig(standAloneSig, forceIsOld: true);
		}
		finally
		{
			standAloneSig.Signature = signature2;
		}
		callConvTokenToSignature.Add(origToken, signature);
		return MDToken.ToRID(origToken);
	}

	private bool IsValidStandAloneSigToken(uint token)
	{
		if (MDToken.ToTable(token) != Table.StandAloneSig)
		{
			return false;
		}
		uint rid = MDToken.ToRID(token);
		return mod.TablesStream.StandAloneSigTable.IsValidRID(rid);
	}

	protected override uint AddMethodSpec(MethodSpec ms)
	{
		return AddMethodSpec(ms, forceIsOld: false);
	}

	private uint AddMethodSpec(MethodSpec ms, bool forceIsOld)
	{
		if (ms == null)
		{
			Error("MethodSpec is null");
			return 0u;
		}
		if (methodSpecInfos.TryGetRid(ms, out var rid))
		{
			return rid;
		}
		bool num = forceIsOld || (base.PreserveMethodSpecRids && mod.ResolveMethodSpec(ms.Rid) == ms);
		RawMethodSpecRow rawMethodSpecRow = new RawMethodSpecRow(AddMethodDefOrRef(ms.Method), GetSignature(ms.Instantiation));
		if (num)
		{
			rid = ms.Rid;
			tablesHeap.MethodSpecTable[ms.Rid] = rawMethodSpecRow;
		}
		else
		{
			rid = tablesHeap.MethodSpecTable.Add(rawMethodSpecRow);
		}
		methodSpecInfos.Add(ms, rid);
		AddCustomAttributes(Table.MethodSpec, rid, ms);
		AddCustomDebugInformationList(Table.MethodSpec, rid, ms);
		return rid;
	}

	protected override void BeforeSortingCustomAttributes()
	{
		InitializeUninitializedTableRows();
	}
}
