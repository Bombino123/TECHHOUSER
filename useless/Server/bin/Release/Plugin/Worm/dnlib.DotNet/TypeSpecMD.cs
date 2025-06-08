using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;

namespace dnlib.DotNet;

internal sealed class TypeSpecMD : TypeSpec, IMDTokenProviderMD, IMDTokenProvider, IContainsGenericParameter2
{
	private readonly ModuleDefMD readerModule;

	private readonly uint origRid;

	private readonly GenericParamContext gpContext;

	private readonly uint signatureOffset;

	public uint OrigRid => origRid;

	bool IContainsGenericParameter2.ContainsGenericParameter => base.ContainsGenericParameter;

	protected override TypeSig GetTypeSigAndExtraData_NoLock(out byte[] extraData)
	{
		TypeSig typeSig = readerModule.ReadTypeSignature(signatureOffset, gpContext, out extraData);
		if (typeSig != null)
		{
			typeSig.Rid = origRid;
		}
		return typeSig;
	}

	protected override void InitializeCustomAttributes()
	{
		RidList list3 = readerModule.Metadata.GetCustomAttributeRidList(Table.TypeSpec, origRid);
		CustomAttributeCollection value = new CustomAttributeCollection(list3.Count, list3, (object list2, int index) => readerModule.ReadCustomAttribute(list3[index]));
		Interlocked.CompareExchange(ref customAttributes, value, null);
	}

	protected override void InitializeCustomDebugInfos()
	{
		List<PdbCustomDebugInfo> list = new List<PdbCustomDebugInfo>();
		readerModule.InitializeCustomDebugInfos(new MDToken(base.MDToken.Table, origRid), gpContext, list);
		Interlocked.CompareExchange(ref customDebugInfos, list, null);
	}

	public TypeSpecMD(ModuleDefMD readerModule, uint rid, GenericParamContext gpContext)
	{
		origRid = rid;
		base.rid = rid;
		this.readerModule = readerModule;
		this.gpContext = gpContext;
		readerModule.TablesStream.TryReadTypeSpecRow(origRid, out var row);
		signatureOffset = row.Signature;
	}
}
