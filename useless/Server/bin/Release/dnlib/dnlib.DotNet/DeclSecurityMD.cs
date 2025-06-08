using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;

namespace dnlib.DotNet;

internal sealed class DeclSecurityMD : DeclSecurity, IMDTokenProviderMD, IMDTokenProvider
{
	private readonly ModuleDefMD readerModule;

	private readonly uint origRid;

	private readonly uint permissionSet;

	public uint OrigRid => origRid;

	protected override void InitializeSecurityAttributes()
	{
		IList<SecurityAttribute> value = DeclSecurityReader.Read(readerModule, permissionSet, default(GenericParamContext));
		Interlocked.CompareExchange(ref securityAttributes, value, null);
	}

	protected override void InitializeCustomAttributes()
	{
		RidList list3 = readerModule.Metadata.GetCustomAttributeRidList(Table.DeclSecurity, origRid);
		CustomAttributeCollection value = new CustomAttributeCollection(list3.Count, list3, (object list2, int index) => readerModule.ReadCustomAttribute(list3[index]));
		Interlocked.CompareExchange(ref customAttributes, value, null);
	}

	protected override void InitializeCustomDebugInfos()
	{
		List<PdbCustomDebugInfo> list = new List<PdbCustomDebugInfo>();
		readerModule.InitializeCustomDebugInfos(new MDToken(base.MDToken.Table, origRid), default(GenericParamContext), list);
		Interlocked.CompareExchange(ref customDebugInfos, list, null);
	}

	public DeclSecurityMD(ModuleDefMD readerModule, uint rid)
	{
		origRid = rid;
		base.rid = rid;
		this.readerModule = readerModule;
		readerModule.TablesStream.TryReadDeclSecurityRow(origRid, out var row);
		permissionSet = row.PermissionSet;
		action = (SecurityAction)row.Action;
	}

	public override byte[] GetBlob()
	{
		return readerModule.BlobStream.Read(permissionSet);
	}
}
