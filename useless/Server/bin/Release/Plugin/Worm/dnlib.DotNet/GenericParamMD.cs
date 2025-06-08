using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;
using dnlib.Utils;

namespace dnlib.DotNet;

internal sealed class GenericParamMD : GenericParam, IMDTokenProviderMD, IMDTokenProvider
{
	private readonly ModuleDefMD readerModule;

	private readonly uint origRid;

	public uint OrigRid => origRid;

	protected override void InitializeCustomAttributes()
	{
		RidList list3 = readerModule.Metadata.GetCustomAttributeRidList(Table.GenericParam, origRid);
		CustomAttributeCollection value = new CustomAttributeCollection(list3.Count, list3, (object list2, int index) => readerModule.ReadCustomAttribute(list3[index]));
		Interlocked.CompareExchange(ref customAttributes, value, null);
	}

	protected override void InitializeCustomDebugInfos()
	{
		List<PdbCustomDebugInfo> list = new List<PdbCustomDebugInfo>();
		readerModule.InitializeCustomDebugInfos(new MDToken(base.MDToken.Table, origRid), GetGenericParamContext(owner), list);
		Interlocked.CompareExchange(ref customDebugInfos, list, null);
	}

	protected override void InitializeGenericParamConstraints()
	{
		RidList genericParamConstraintRidList = readerModule.Metadata.GetGenericParamConstraintRidList(origRid);
		LazyList<GenericParamConstraint, RidList> value = new LazyList<GenericParamConstraint, RidList>(genericParamConstraintRidList.Count, this, genericParamConstraintRidList, (RidList list2, int index) => readerModule.ResolveGenericParamConstraint(list2[index], GetGenericParamContext(owner)));
		Interlocked.CompareExchange(ref genericParamConstraints, value, null);
	}

	private static GenericParamContext GetGenericParamContext(ITypeOrMethodDef tmOwner)
	{
		if (tmOwner is MethodDef method)
		{
			return GenericParamContext.Create(method);
		}
		return new GenericParamContext(tmOwner as TypeDef);
	}

	public GenericParamMD(ModuleDefMD readerModule, uint rid)
	{
		origRid = rid;
		base.rid = rid;
		this.readerModule = readerModule;
		readerModule.TablesStream.TryReadGenericParamRow(origRid, out var row);
		number = row.Number;
		attributes = row.Flags;
		name = readerModule.StringsStream.ReadNoNull(row.Name);
		owner = readerModule.GetOwner(this);
		if (row.Kind != 0)
		{
			kind = readerModule.ResolveTypeDefOrRef(row.Kind, GetGenericParamContext(owner));
		}
	}

	internal GenericParamMD InitializeAll()
	{
		MemberMDInitializer.Initialize(base.Owner);
		MemberMDInitializer.Initialize(base.Number);
		MemberMDInitializer.Initialize(base.Flags);
		MemberMDInitializer.Initialize(base.Name);
		MemberMDInitializer.Initialize(base.Kind);
		MemberMDInitializer.Initialize(base.CustomAttributes);
		MemberMDInitializer.Initialize(base.GenericParamConstraints);
		return this;
	}

	internal override void OnLazyAdd2(int index, ref GenericParamConstraint value)
	{
		if (value.Owner != this)
		{
			value = readerModule.ForceUpdateRowId(readerModule.ReadGenericParamConstraint(value.Rid, GetGenericParamContext(owner)).InitializeAll());
			value.Owner = this;
		}
	}
}
