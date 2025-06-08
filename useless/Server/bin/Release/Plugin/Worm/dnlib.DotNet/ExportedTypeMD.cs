using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;

namespace dnlib.DotNet;

internal sealed class ExportedTypeMD : ExportedType, IMDTokenProviderMD, IMDTokenProvider
{
	private readonly ModuleDefMD readerModule;

	private readonly uint origRid;

	private readonly uint implementationRid;

	public uint OrigRid => origRid;

	protected override void InitializeCustomAttributes()
	{
		RidList list3 = readerModule.Metadata.GetCustomAttributeRidList(Table.ExportedType, origRid);
		CustomAttributeCollection value = new CustomAttributeCollection(list3.Count, list3, (object list2, int index) => readerModule.ReadCustomAttribute(list3[index]));
		Interlocked.CompareExchange(ref customAttributes, value, null);
	}

	protected override void InitializeCustomDebugInfos()
	{
		List<PdbCustomDebugInfo> list = new List<PdbCustomDebugInfo>();
		readerModule.InitializeCustomDebugInfos(new MDToken(base.MDToken.Table, origRid), default(GenericParamContext), list);
		Interlocked.CompareExchange(ref customDebugInfos, list, null);
	}

	protected override IImplementation GetImplementation_NoLock()
	{
		return readerModule.ResolveImplementation(implementationRid);
	}

	public ExportedTypeMD(ModuleDefMD readerModule, uint rid)
	{
		origRid = rid;
		base.rid = rid;
		this.readerModule = readerModule;
		module = readerModule;
		readerModule.TablesStream.TryReadExportedTypeRow(origRid, out var row);
		implementationRid = row.Implementation;
		attributes = (int)row.Flags;
		typeDefId = row.TypeDefId;
		typeName = readerModule.StringsStream.ReadNoNull(row.TypeName);
		typeNamespace = readerModule.StringsStream.ReadNoNull(row.TypeNamespace);
	}
}
