using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;

namespace dnlib.DotNet;

internal sealed class TypeRefMD : TypeRef, IMDTokenProviderMD, IMDTokenProvider
{
	private readonly ModuleDefMD readerModule;

	private readonly uint origRid;

	private readonly uint resolutionScopeCodedToken;

	public uint OrigRid => origRid;

	protected override IResolutionScope GetResolutionScope_NoLock()
	{
		return readerModule.ResolveResolutionScope(resolutionScopeCodedToken);
	}

	protected override void InitializeCustomAttributes()
	{
		RidList list3 = readerModule.Metadata.GetCustomAttributeRidList(Table.TypeRef, origRid);
		CustomAttributeCollection value = new CustomAttributeCollection(list3.Count, list3, (object list2, int index) => readerModule.ReadCustomAttribute(list3[index]));
		Interlocked.CompareExchange(ref customAttributes, value, null);
	}

	protected override void InitializeCustomDebugInfos()
	{
		List<PdbCustomDebugInfo> list = new List<PdbCustomDebugInfo>();
		readerModule.InitializeCustomDebugInfos(new MDToken(base.MDToken.Table, origRid), default(GenericParamContext), list);
		Interlocked.CompareExchange(ref customDebugInfos, list, null);
	}

	public TypeRefMD(ModuleDefMD readerModule, uint rid)
	{
		origRid = rid;
		base.rid = rid;
		this.readerModule = readerModule;
		module = readerModule;
		readerModule.TablesStream.TryReadTypeRefRow(origRid, out var row);
		name = readerModule.StringsStream.ReadNoNull(row.Name);
		@namespace = readerModule.StringsStream.ReadNoNull(row.Namespace);
		resolutionScopeCodedToken = row.ResolutionScope;
	}
}
