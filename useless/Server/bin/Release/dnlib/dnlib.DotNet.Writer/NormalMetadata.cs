using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet.MD;

namespace dnlib.DotNet.Writer;

internal sealed class NormalMetadata : Metadata
{
	private readonly Rows<TypeRef> typeRefInfos = new Rows<TypeRef>();

	private readonly Rows<TypeDef> typeDefInfos = new Rows<TypeDef>();

	private readonly Rows<FieldDef> fieldDefInfos = new Rows<FieldDef>();

	private readonly Rows<MethodDef> methodDefInfos = new Rows<MethodDef>();

	private readonly Rows<ParamDef> paramDefInfos = new Rows<ParamDef>();

	private readonly Rows<MemberRef> memberRefInfos = new Rows<MemberRef>();

	private readonly Rows<StandAloneSig> standAloneSigInfos = new Rows<StandAloneSig>();

	private readonly Rows<EventDef> eventDefInfos = new Rows<EventDef>();

	private readonly Rows<PropertyDef> propertyDefInfos = new Rows<PropertyDef>();

	private readonly Rows<TypeSpec> typeSpecInfos = new Rows<TypeSpec>();

	private readonly Rows<MethodSpec> methodSpecInfos = new Rows<MethodSpec>();

	protected override int NumberOfMethods => methodDefInfos.Count;

	public NormalMetadata(ModuleDef module, UniqueChunkList<ByteArrayChunk> constants, MethodBodyChunks methodBodies, NetResources netResources, MetadataOptions options, DebugMetadataKind debugKind, bool isStandaloneDebugMetadata)
		: base(module, constants, methodBodies, netResources, options, debugKind, isStandaloneDebugMetadata)
	{
	}

	protected override TypeDef[] GetAllTypeDefs()
	{
		return module.GetTypes().ToArray();
	}

	protected override void AllocateTypeDefRids()
	{
		TypeDef[] array = allTypeDefs;
		foreach (TypeDef typeDef in array)
		{
			if (typeDef != null)
			{
				uint rid = tablesHeap.TypeDefTable.Create(default(RawTypeDefRow));
				typeDefInfos.Add(typeDef, rid);
			}
		}
	}

	protected override void AllocateMemberDefRids()
	{
		int num = allTypeDefs.Length;
		int num2 = 0;
		int num3 = 0;
		int num4 = num / 5;
		uint fieldList = 1u;
		uint methodList = 1u;
		uint eventList = 1u;
		uint propertyList = 1u;
		uint paramList = 1u;
		TypeDef[] array = allTypeDefs;
		foreach (TypeDef typeDef in array)
		{
			if (num2++ == num4 && num3 < 5)
			{
				RaiseProgress(dnlib.DotNet.Writer.MetadataEvent.AllocateMemberDefRids, (double)num2 / (double)num);
				num3++;
				num4 = (int)((double)num / 5.0 * (double)(num3 + 1));
			}
			if (typeDef == null)
			{
				continue;
			}
			uint rid = GetRid(typeDef);
			RawTypeDefRow rawTypeDefRow = tablesHeap.TypeDefTable[rid];
			rawTypeDefRow = new RawTypeDefRow(rawTypeDefRow.Flags, rawTypeDefRow.Name, rawTypeDefRow.Namespace, rawTypeDefRow.Extends, fieldList, methodList);
			tablesHeap.TypeDefTable[rid] = rawTypeDefRow;
			IList<FieldDef> fields = typeDef.Fields;
			int count = fields.Count;
			for (int j = 0; j < count; j++)
			{
				FieldDef fieldDef = fields[j];
				if (fieldDef != null)
				{
					uint num5 = fieldList++;
					if (num5 != tablesHeap.FieldTable.Create(default(RawFieldRow)))
					{
						throw new ModuleWriterException("Invalid field rid");
					}
					fieldDefInfos.Add(fieldDef, num5);
				}
			}
			IList<MethodDef> methods = typeDef.Methods;
			count = methods.Count;
			for (int k = 0; k < count; k++)
			{
				MethodDef methodDef = methods[k];
				if (methodDef == null)
				{
					continue;
				}
				uint num6 = methodList++;
				RawMethodRow row = new RawMethodRow(0u, 0, 0, 0u, 0u, paramList);
				if (num6 != tablesHeap.MethodTable.Create(row))
				{
					throw new ModuleWriterException("Invalid method rid");
				}
				methodDefInfos.Add(methodDef, num6);
				foreach (ParamDef item in Metadata.Sort(methodDef.ParamDefs))
				{
					if (item != null)
					{
						uint num7 = paramList++;
						if (num7 != tablesHeap.ParamTable.Create(default(RawParamRow)))
						{
							throw new ModuleWriterException("Invalid param rid");
						}
						paramDefInfos.Add(item, num7);
					}
				}
			}
			if (!Metadata.IsEmpty(typeDef.Events))
			{
				uint rid2 = tablesHeap.EventMapTable.Create(new RawEventMapRow(rid, eventList));
				eventMapInfos.Add(typeDef, rid2);
				IList<EventDef> events = typeDef.Events;
				count = events.Count;
				for (int l = 0; l < count; l++)
				{
					EventDef eventDef = events[l];
					if (eventDef != null)
					{
						uint num8 = eventList++;
						if (num8 != tablesHeap.EventTable.Create(default(RawEventRow)))
						{
							throw new ModuleWriterException("Invalid event rid");
						}
						eventDefInfos.Add(eventDef, num8);
					}
				}
			}
			if (Metadata.IsEmpty(typeDef.Properties))
			{
				continue;
			}
			uint rid3 = tablesHeap.PropertyMapTable.Create(new RawPropertyMapRow(rid, propertyList));
			propertyMapInfos.Add(typeDef, rid3);
			IList<PropertyDef> properties = typeDef.Properties;
			count = properties.Count;
			for (int m = 0; m < count; m++)
			{
				PropertyDef propertyDef = properties[m];
				if (propertyDef != null)
				{
					uint num9 = propertyList++;
					if (num9 != tablesHeap.PropertyTable.Create(default(RawPropertyRow)))
					{
						throw new ModuleWriterException("Invalid property rid");
					}
					propertyDefInfos.Add(propertyDef, num9);
				}
			}
		}
	}

	public override uint GetRid(TypeRef tr)
	{
		typeRefInfos.TryGetRid(tr, out var rid);
		return rid;
	}

	public override uint GetRid(TypeDef td)
	{
		if (typeDefInfos.TryGetRid(td, out var rid))
		{
			return rid;
		}
		if (td == null)
		{
			Error("TypeDef is null");
		}
		else
		{
			Error("TypeDef '{0}' (0x{1:X8}) is not defined in this module '{2}'. A type was removed that is still referenced by this module.", td, td.MDToken.Raw, module);
		}
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
		RawTypeRefRow row = new RawTypeRefRow(AddResolutionScope(tr.ResolutionScope), stringsHeap.Add(tr.Name), stringsHeap.Add(tr.Namespace));
		rid = tablesHeap.TypeRefTable.Add(row);
		typeRefInfos.SetRid(tr, rid);
		AddCustomAttributes(Table.TypeRef, rid, tr);
		AddCustomDebugInformationList(Table.TypeRef, rid, tr);
		return rid;
	}

	protected override uint AddTypeSpec(TypeSpec ts)
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
		RawTypeSpecRow row = new RawTypeSpecRow(GetSignature(ts.TypeSig, ts.ExtraData));
		rid = tablesHeap.TypeSpecTable.Add(row);
		typeSpecInfos.SetRid(ts, rid);
		AddCustomAttributes(Table.TypeSpec, rid, ts);
		AddCustomDebugInformationList(Table.TypeSpec, rid, ts);
		return rid;
	}

	protected override uint AddMemberRef(MemberRef mr)
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
		RawMemberRefRow row = new RawMemberRefRow(AddMemberRefParent(mr.Class), stringsHeap.Add(mr.Name), GetSignature(mr.Signature));
		rid = tablesHeap.MemberRefTable.Add(row);
		memberRefInfos.Add(mr, rid);
		AddCustomAttributes(Table.MemberRef, rid, mr);
		AddCustomDebugInformationList(Table.MemberRef, rid, mr);
		return rid;
	}

	protected override uint AddStandAloneSig(StandAloneSig sas)
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
		RawStandAloneSigRow row = new RawStandAloneSigRow(GetSignature(sas.Signature));
		rid = tablesHeap.StandAloneSigTable.Add(row);
		standAloneSigInfos.Add(sas, rid);
		AddCustomAttributes(Table.StandAloneSig, rid, sas);
		AddCustomDebugInformationList(Table.StandAloneSig, rid, sas);
		return rid;
	}

	protected override uint AddMethodSpec(MethodSpec ms)
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
		RawMethodSpecRow row = new RawMethodSpecRow(AddMethodDefOrRef(ms.Method), GetSignature(ms.Instantiation));
		rid = tablesHeap.MethodSpecTable.Add(row);
		methodSpecInfos.Add(ms, rid);
		AddCustomAttributes(Table.MethodSpec, rid, ms);
		AddCustomDebugInformationList(Table.MethodSpec, rid, ms);
		return rid;
	}
}
