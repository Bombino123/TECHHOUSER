using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;

namespace dnlib.DotNet;

internal sealed class StandAloneSigMD : StandAloneSig, IMDTokenProviderMD, IMDTokenProvider, IContainsGenericParameter2
{
	private readonly ModuleDefMD readerModule;

	private readonly uint origRid;

	private readonly GenericParamContext gpContext;

	public uint OrigRid => origRid;

	protected override void InitializeCustomAttributes()
	{
		RidList list3 = readerModule.Metadata.GetCustomAttributeRidList(Table.StandAloneSig, origRid);
		CustomAttributeCollection value = new CustomAttributeCollection(list3.Count, list3, (object list2, int index) => readerModule.ReadCustomAttribute(list3[index]));
		Interlocked.CompareExchange(ref customAttributes, value, null);
	}

	protected override void InitializeCustomDebugInfos()
	{
		List<PdbCustomDebugInfo> list = new List<PdbCustomDebugInfo>();
		readerModule.InitializeCustomDebugInfos(new MDToken(base.MDToken.Table, origRid), gpContext, list);
		Interlocked.CompareExchange(ref customDebugInfos, list, null);
	}

	public StandAloneSigMD(ModuleDefMD readerModule, uint rid, GenericParamContext gpContext)
	{
		origRid = rid;
		base.rid = rid;
		this.readerModule = readerModule;
		this.gpContext = gpContext;
		readerModule.TablesStream.TryReadStandAloneSigRow(origRid, out var row);
		signature = readerModule.ReadSignature(row.Signature, gpContext);
	}
}
