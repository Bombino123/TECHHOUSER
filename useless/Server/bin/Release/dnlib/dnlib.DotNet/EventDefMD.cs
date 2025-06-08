using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;

namespace dnlib.DotNet;

internal sealed class EventDefMD : EventDef, IMDTokenProviderMD, IMDTokenProvider
{
	private readonly ModuleDefMD readerModule;

	private readonly uint origRid;

	public uint OrigRid => origRid;

	protected override void InitializeCustomAttributes()
	{
		RidList list3 = readerModule.Metadata.GetCustomAttributeRidList(Table.Event, origRid);
		CustomAttributeCollection value = new CustomAttributeCollection(list3.Count, list3, (object list2, int index) => readerModule.ReadCustomAttribute(list3[index]));
		Interlocked.CompareExchange(ref customAttributes, value, null);
	}

	protected override void InitializeCustomDebugInfos()
	{
		List<PdbCustomDebugInfo> list = new List<PdbCustomDebugInfo>();
		readerModule.InitializeCustomDebugInfos(new MDToken(base.MDToken.Table, origRid), new GenericParamContext(declaringType2), list);
		Interlocked.CompareExchange(ref customDebugInfos, list, null);
	}

	public EventDefMD(ModuleDefMD readerModule, uint rid)
	{
		origRid = rid;
		base.rid = rid;
		this.readerModule = readerModule;
		readerModule.TablesStream.TryReadEventRow(origRid, out var row);
		attributes = row.EventFlags;
		name = readerModule.StringsStream.ReadNoNull(row.Name);
		declaringType2 = readerModule.GetOwnerType(this);
		eventType = readerModule.ResolveTypeDefOrRef(row.EventType, new GenericParamContext(declaringType2));
	}

	internal EventDefMD InitializeAll()
	{
		MemberMDInitializer.Initialize(base.Attributes);
		MemberMDInitializer.Initialize(base.Name);
		MemberMDInitializer.Initialize(base.EventType);
		MemberMDInitializer.Initialize(base.CustomAttributes);
		MemberMDInitializer.Initialize(base.AddMethod);
		MemberMDInitializer.Initialize(base.InvokeMethod);
		MemberMDInitializer.Initialize(base.RemoveMethod);
		MemberMDInitializer.Initialize(base.OtherMethods);
		MemberMDInitializer.Initialize(base.DeclaringType);
		return this;
	}

	protected override void InitializeEventMethods_NoLock()
	{
		IList<MethodDef> list;
		if (!(declaringType2 is TypeDefMD typeDefMD))
		{
			list = new List<MethodDef>();
		}
		else
		{
			typeDefMD.InitializeEvent(this, out addMethod, out invokeMethod, out removeMethod, out list);
		}
		otherMethods = list;
	}
}
