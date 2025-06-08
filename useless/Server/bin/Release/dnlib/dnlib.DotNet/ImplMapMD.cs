namespace dnlib.DotNet;

internal sealed class ImplMapMD : ImplMap, IMDTokenProviderMD, IMDTokenProvider
{
	private readonly uint origRid;

	public uint OrigRid => origRid;

	public ImplMapMD(ModuleDefMD readerModule, uint rid)
	{
		origRid = rid;
		base.rid = rid;
		readerModule.TablesStream.TryReadImplMapRow(origRid, out var row);
		attributes = row.MappingFlags;
		name = readerModule.StringsStream.ReadNoNull(row.ImportName);
		module = readerModule.ResolveModuleRef(row.ImportScope);
	}
}
