using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;

namespace dnlib.DotNet;

internal sealed class MemberRefMD : MemberRef, IMDTokenProviderMD, IMDTokenProvider, IContainsGenericParameter2
{
	private readonly ModuleDefMD readerModule;

	private readonly uint origRid;

	private readonly GenericParamContext gpContext;

	public uint OrigRid => origRid;

	bool IContainsGenericParameter2.ContainsGenericParameter => TypeHelper.ContainsGenericParameter(this);

	protected override void InitializeCustomAttributes()
	{
		RidList list3 = readerModule.Metadata.GetCustomAttributeRidList(Table.MemberRef, origRid);
		CustomAttributeCollection value = new CustomAttributeCollection(list3.Count, list3, (object list2, int index) => readerModule.ReadCustomAttribute(list3[index]));
		Interlocked.CompareExchange(ref customAttributes, value, null);
	}

	protected override void InitializeCustomDebugInfos()
	{
		List<PdbCustomDebugInfo> list = new List<PdbCustomDebugInfo>();
		readerModule.InitializeCustomDebugInfos(new MDToken(base.MDToken.Table, origRid), gpContext, list);
		Interlocked.CompareExchange(ref customDebugInfos, list, null);
	}

	public MemberRefMD(ModuleDefMD readerModule, uint rid, GenericParamContext gpContext)
	{
		origRid = rid;
		base.rid = rid;
		this.readerModule = readerModule;
		this.gpContext = gpContext;
		module = readerModule;
		readerModule.TablesStream.TryReadMemberRefRow(origRid, out var row);
		name = readerModule.StringsStream.ReadNoNull(row.Name);
		@class = readerModule.ResolveMemberRefParent(row.Class, gpContext);
		signature = readerModule.ReadSignature(row.Signature, MemberRef.GetSignatureGenericParamContext(gpContext, @class));
	}
}
