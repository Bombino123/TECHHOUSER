using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;

namespace dnlib.DotNet;

internal sealed class PropertyDefMD : PropertyDef, IMDTokenProviderMD, IMDTokenProvider
{
	private readonly ModuleDefMD readerModule;

	private readonly uint origRid;

	public uint OrigRid => origRid;

	protected override Constant GetConstant_NoLock()
	{
		return readerModule.ResolveConstant(readerModule.Metadata.GetConstantRid(Table.Property, origRid));
	}

	protected override void InitializeCustomAttributes()
	{
		RidList list3 = readerModule.Metadata.GetCustomAttributeRidList(Table.Property, origRid);
		CustomAttributeCollection value = new CustomAttributeCollection(list3.Count, list3, (object list2, int index) => readerModule.ReadCustomAttribute(list3[index]));
		Interlocked.CompareExchange(ref customAttributes, value, null);
	}

	protected override void InitializeCustomDebugInfos()
	{
		List<PdbCustomDebugInfo> list = new List<PdbCustomDebugInfo>();
		readerModule.InitializeCustomDebugInfos(new MDToken(base.MDToken.Table, origRid), new GenericParamContext(declaringType2), list);
		Interlocked.CompareExchange(ref customDebugInfos, list, null);
	}

	public PropertyDefMD(ModuleDefMD readerModule, uint rid)
	{
		origRid = rid;
		base.rid = rid;
		this.readerModule = readerModule;
		readerModule.TablesStream.TryReadPropertyRow(origRid, out var row);
		attributes = row.PropFlags;
		name = readerModule.StringsStream.ReadNoNull(row.Name);
		declaringType2 = readerModule.GetOwnerType(this);
		type = readerModule.ReadSignature(row.Type, new GenericParamContext(declaringType2));
	}

	internal PropertyDefMD InitializeAll()
	{
		MemberMDInitializer.Initialize(base.Attributes);
		MemberMDInitializer.Initialize(base.Name);
		MemberMDInitializer.Initialize(base.Type);
		MemberMDInitializer.Initialize(base.Constant);
		MemberMDInitializer.Initialize(base.CustomAttributes);
		MemberMDInitializer.Initialize(base.GetMethod);
		MemberMDInitializer.Initialize(base.SetMethod);
		MemberMDInitializer.Initialize(base.OtherMethods);
		MemberMDInitializer.Initialize(base.DeclaringType);
		return this;
	}

	protected override void InitializePropertyMethods_NoLock()
	{
		if (otherMethods == null)
		{
			IList<MethodDef> list;
			IList<MethodDef> list2;
			IList<MethodDef> list3;
			if (!(declaringType2 is TypeDefMD typeDefMD))
			{
				list = new List<MethodDef>();
				list2 = new List<MethodDef>();
				list3 = new List<MethodDef>();
			}
			else
			{
				typeDefMD.InitializeProperty(this, out list, out list2, out list3);
			}
			getMethods = list;
			setMethods = list2;
			otherMethods = list3;
		}
	}
}
