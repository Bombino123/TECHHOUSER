using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet.Emit;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;
using dnlib.PE;
using dnlib.Utils;

namespace dnlib.DotNet;

internal sealed class MethodDefMD : MethodDef, IMDTokenProviderMD, IMDTokenProvider
{
	private readonly ModuleDefMD readerModule;

	private readonly uint origRid;

	private readonly RVA origRva;

	private readonly MethodImplAttributes origImplAttributes;

	public uint OrigRid => origRid;

	protected override void InitializeParamDefs()
	{
		RidList paramRidList = readerModule.Metadata.GetParamRidList(origRid);
		LazyList<ParamDef, RidList> value = new LazyList<ParamDef, RidList>(paramRidList.Count, this, paramRidList, (RidList list2, int index) => readerModule.ResolveParam(list2[index]));
		Interlocked.CompareExchange(ref paramDefs, value, null);
	}

	protected override void InitializeGenericParameters()
	{
		RidList genericParamRidList = readerModule.Metadata.GetGenericParamRidList(Table.Method, origRid);
		LazyList<GenericParam, RidList> value = new LazyList<GenericParam, RidList>(genericParamRidList.Count, this, genericParamRidList, (RidList list2, int index) => readerModule.ResolveGenericParam(list2[index]));
		Interlocked.CompareExchange(ref genericParameters, value, null);
	}

	protected override void InitializeDeclSecurities()
	{
		RidList declSecurityRidList = readerModule.Metadata.GetDeclSecurityRidList(Table.Method, origRid);
		LazyList<DeclSecurity, RidList> value = new LazyList<DeclSecurity, RidList>(declSecurityRidList.Count, declSecurityRidList, (RidList list2, int index) => readerModule.ResolveDeclSecurity(list2[index]));
		Interlocked.CompareExchange(ref declSecurities, value, null);
	}

	protected override ImplMap GetImplMap_NoLock()
	{
		return readerModule.ResolveImplMap(readerModule.Metadata.GetImplMapRid(Table.Method, origRid));
	}

	protected override MethodBody GetMethodBody_NoLock()
	{
		return readerModule.ReadMethodBody(this, origRva, origImplAttributes, new GenericParamContext(declaringType2, this));
	}

	protected override void InitializeCustomAttributes()
	{
		RidList list3 = readerModule.Metadata.GetCustomAttributeRidList(Table.Method, origRid);
		CustomAttributeCollection value = new CustomAttributeCollection(list3.Count, list3, (object list2, int index) => readerModule.ReadCustomAttribute(list3[index]));
		Interlocked.CompareExchange(ref customAttributes, value, null);
	}

	protected override void InitializeCustomDebugInfos()
	{
		List<PdbCustomDebugInfo> value = new List<PdbCustomDebugInfo>();
		if (Interlocked.CompareExchange(ref customDebugInfos, value, null) == null)
		{
			CilBody body = base.Body;
			readerModule.InitializeCustomDebugInfos(this, body, value);
		}
	}

	protected override void InitializeOverrides()
	{
		IList<MethodOverride> list;
		if (declaringType2 is TypeDefMD typeDefMD)
		{
			list = typeDefMD.GetMethodOverrides(this, new GenericParamContext(declaringType2, this));
		}
		else
		{
			IList<MethodOverride> list2 = new List<MethodOverride>();
			list = list2;
		}
		IList<MethodOverride> value = list;
		Interlocked.CompareExchange(ref overrides, value, null);
	}

	protected override void InitializeSemanticsAttributes()
	{
		if (base.DeclaringType is TypeDefMD typeDefMD)
		{
			typeDefMD.InitializeMethodSemanticsAttributes();
		}
		semAttrs |= MethodDef.SEMATTRS_INITD;
	}

	public MethodDefMD(ModuleDefMD readerModule, uint rid)
	{
		origRid = rid;
		base.rid = rid;
		this.readerModule = readerModule;
		readerModule.TablesStream.TryReadMethodRow(origRid, out var row);
		rva = (RVA)row.RVA;
		implAttributes = row.ImplFlags;
		attributes = row.Flags;
		name = readerModule.StringsStream.ReadNoNull(row.Name);
		origRva = rva;
		origImplAttributes = (MethodImplAttributes)implAttributes;
		declaringType2 = readerModule.GetOwnerType(this);
		signature = readerModule.ReadSignature(row.Signature, new GenericParamContext(declaringType2, this));
		parameterList = new ParameterList(this, declaringType2);
		exportInfo = readerModule.GetExportInfo(rid);
	}

	internal MethodDefMD InitializeAll()
	{
		MemberMDInitializer.Initialize(base.RVA);
		MemberMDInitializer.Initialize(base.Attributes);
		MemberMDInitializer.Initialize(base.ImplAttributes);
		MemberMDInitializer.Initialize(base.Name);
		MemberMDInitializer.Initialize(base.Signature);
		MemberMDInitializer.Initialize(base.ImplMap);
		MemberMDInitializer.Initialize(base.MethodBody);
		MemberMDInitializer.Initialize(base.DeclaringType);
		MemberMDInitializer.Initialize(base.CustomAttributes);
		MemberMDInitializer.Initialize(base.Overrides);
		MemberMDInitializer.Initialize(base.ParamDefs);
		MemberMDInitializer.Initialize(base.GenericParameters);
		MemberMDInitializer.Initialize(base.DeclSecurities);
		return this;
	}

	internal override void OnLazyAdd2(int index, ref GenericParam value)
	{
		if (value.Owner != this)
		{
			value = readerModule.ForceUpdateRowId(readerModule.ReadGenericParam(value.Rid).InitializeAll());
			value.Owner = this;
		}
	}

	internal override void OnLazyAdd2(int index, ref ParamDef value)
	{
		if (value.DeclaringMethod != this)
		{
			value = readerModule.ForceUpdateRowId(readerModule.ReadParam(value.Rid).InitializeAll());
			value.DeclaringMethod = this;
		}
	}
}
