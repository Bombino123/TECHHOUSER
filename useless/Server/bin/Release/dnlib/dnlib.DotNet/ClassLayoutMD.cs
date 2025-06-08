namespace dnlib.DotNet;

internal sealed class ClassLayoutMD : ClassLayout, IMDTokenProviderMD, IMDTokenProvider
{
	private readonly uint origRid;

	public uint OrigRid => origRid;

	public ClassLayoutMD(ModuleDefMD readerModule, uint rid)
	{
		origRid = rid;
		base.rid = rid;
		readerModule.TablesStream.TryReadClassLayoutRow(origRid, out var row);
		classSize = row.ClassSize;
		packingSize = row.PackingSize;
	}
}
