using System;
using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;

namespace dnlib.DotNet;

internal sealed class AssemblyRefMD : AssemblyRef, IMDTokenProviderMD, IMDTokenProvider
{
	private readonly ModuleDefMD readerModule;

	private readonly uint origRid;

	public uint OrigRid => origRid;

	protected override void InitializeCustomAttributes()
	{
		RidList list3 = readerModule.Metadata.GetCustomAttributeRidList(Table.AssemblyRef, origRid);
		CustomAttributeCollection value = new CustomAttributeCollection(list3.Count, list3, (object list2, int index) => readerModule.ReadCustomAttribute(list3[index]));
		Interlocked.CompareExchange(ref customAttributes, value, null);
	}

	protected override void InitializeCustomDebugInfos()
	{
		List<PdbCustomDebugInfo> list = new List<PdbCustomDebugInfo>();
		readerModule.InitializeCustomDebugInfos(new MDToken(base.MDToken.Table, origRid), default(GenericParamContext), list);
		Interlocked.CompareExchange(ref customDebugInfos, list, null);
	}

	public AssemblyRefMD(ModuleDefMD readerModule, uint rid)
	{
		origRid = rid;
		base.rid = rid;
		this.readerModule = readerModule;
		readerModule.TablesStream.TryReadAssemblyRefRow(origRid, out var row);
		version = new Version(row.MajorVersion, row.MinorVersion, row.BuildNumber, row.RevisionNumber);
		attributes = (int)row.Flags;
		byte[] data = readerModule.BlobStream.Read(row.PublicKeyOrToken);
		if (((ulong)attributes & 1uL) != 0L)
		{
			publicKeyOrToken = new PublicKey(data);
		}
		else
		{
			publicKeyOrToken = new PublicKeyToken(data);
		}
		name = readerModule.StringsStream.ReadNoNull(row.Name);
		culture = readerModule.StringsStream.ReadNoNull(row.Locale);
		hashValue = readerModule.BlobStream.Read(row.HashValue);
	}
}
