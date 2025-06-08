using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;
using dnlib.PE;

namespace dnlib.DotNet;

internal sealed class FieldDefMD : FieldDef, IMDTokenProviderMD, IMDTokenProvider
{
	private readonly ModuleDefMD readerModule;

	private readonly uint origRid;

	private readonly FieldAttributes origAttributes;

	public uint OrigRid => origRid;

	protected override void InitializeCustomAttributes()
	{
		RidList list3 = readerModule.Metadata.GetCustomAttributeRidList(Table.Field, origRid);
		CustomAttributeCollection value = new CustomAttributeCollection(list3.Count, list3, (object list2, int index) => readerModule.ReadCustomAttribute(list3[index]));
		Interlocked.CompareExchange(ref customAttributes, value, null);
	}

	protected override void InitializeCustomDebugInfos()
	{
		List<PdbCustomDebugInfo> list = new List<PdbCustomDebugInfo>();
		readerModule.InitializeCustomDebugInfos(new MDToken(base.MDToken.Table, origRid), new GenericParamContext(declaringType2), list);
		Interlocked.CompareExchange(ref customDebugInfos, list, null);
	}

	protected override uint? GetFieldOffset_NoLock()
	{
		if (readerModule.TablesStream.TryReadFieldLayoutRow(readerModule.Metadata.GetFieldLayoutRid(origRid), out var row))
		{
			return row.OffSet;
		}
		return null;
	}

	protected override MarshalType GetMarshalType_NoLock()
	{
		return readerModule.ReadMarshalType(Table.Field, origRid, new GenericParamContext(declaringType2));
	}

	protected override RVA GetRVA_NoLock()
	{
		GetFieldRVA_NoLock(out var result);
		return result;
	}

	protected override byte[] GetInitialValue_NoLock()
	{
		if (!GetFieldRVA_NoLock(out var rVA))
		{
			return null;
		}
		return ReadInitialValue_NoLock(rVA);
	}

	protected override ImplMap GetImplMap_NoLock()
	{
		return readerModule.ResolveImplMap(readerModule.Metadata.GetImplMapRid(Table.Field, origRid));
	}

	protected override Constant GetConstant_NoLock()
	{
		return readerModule.ResolveConstant(readerModule.Metadata.GetConstantRid(Table.Field, origRid));
	}

	public FieldDefMD(ModuleDefMD readerModule, uint rid)
	{
		origRid = rid;
		base.rid = rid;
		this.readerModule = readerModule;
		readerModule.TablesStream.TryReadFieldRow(origRid, out var row);
		name = readerModule.StringsStream.ReadNoNull(row.Name);
		attributes = row.Flags;
		origAttributes = (FieldAttributes)attributes;
		declaringType2 = readerModule.GetOwnerType(this);
		signature = readerModule.ReadSignature(row.Signature, new GenericParamContext(declaringType2));
	}

	internal FieldDefMD InitializeAll()
	{
		MemberMDInitializer.Initialize(base.CustomAttributes);
		MemberMDInitializer.Initialize(base.Attributes);
		MemberMDInitializer.Initialize(base.Name);
		MemberMDInitializer.Initialize(base.Signature);
		MemberMDInitializer.Initialize(base.FieldOffset);
		MemberMDInitializer.Initialize(base.MarshalType);
		MemberMDInitializer.Initialize(base.RVA);
		MemberMDInitializer.Initialize(base.InitialValue);
		MemberMDInitializer.Initialize(base.ImplMap);
		MemberMDInitializer.Initialize(base.Constant);
		MemberMDInitializer.Initialize(base.DeclaringType);
		return this;
	}

	private bool GetFieldRVA_NoLock(out RVA rva)
	{
		if ((origAttributes & FieldAttributes.HasFieldRVA) == 0)
		{
			rva = (RVA)0u;
			return false;
		}
		if (!readerModule.TablesStream.TryReadFieldRVARow(readerModule.Metadata.GetFieldRVARid(origRid), out var row))
		{
			rva = (RVA)0u;
			return false;
		}
		rva = (RVA)row.RVA;
		return true;
	}

	private byte[] ReadInitialValue_NoLock(RVA rva)
	{
		if (!GetFieldSize(declaringType2, signature as FieldSig, out var size))
		{
			return null;
		}
		if (size >= int.MaxValue)
		{
			return null;
		}
		return readerModule.ReadDataAt(rva, (int)size);
	}
}
