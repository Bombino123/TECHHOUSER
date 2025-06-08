using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;
using dnlib.IO;

namespace dnlib.DotNet;

internal sealed class EmbeddedResourceMD : EmbeddedResource, IMDTokenProviderMD, IMDTokenProvider
{
	private readonly ModuleDefMD readerModule;

	private readonly uint origRid;

	public uint OrigRid => origRid;

	protected override void InitializeCustomAttributes()
	{
		RidList list3 = readerModule.Metadata.GetCustomAttributeRidList(Table.ManifestResource, origRid);
		CustomAttributeCollection value = new CustomAttributeCollection(list3.Count, list3, (object list2, int index) => readerModule.ReadCustomAttribute(list3[index]));
		Interlocked.CompareExchange(ref customAttributes, value, null);
	}

	protected override void InitializeCustomDebugInfos()
	{
		List<PdbCustomDebugInfo> list = new List<PdbCustomDebugInfo>();
		readerModule.InitializeCustomDebugInfos(new MDToken(base.MDToken.Table, origRid), default(GenericParamContext), list);
		Interlocked.CompareExchange(ref customDebugInfos, list, null);
	}

	public EmbeddedResourceMD(ModuleDefMD readerModule, ManifestResource mr, byte[] data)
		: this(readerModule, mr, ByteArrayDataReaderFactory.Create(data, null), 0u, (uint)data.Length)
	{
	}

	public EmbeddedResourceMD(ModuleDefMD readerModule, ManifestResource mr, DataReaderFactory dataReaderFactory, uint offset, uint length)
		: base(mr.Name, dataReaderFactory, offset, length, mr.Flags)
	{
		this.readerModule = readerModule;
		origRid = (rid = mr.Rid);
		base.offset = mr.Offset;
	}
}
