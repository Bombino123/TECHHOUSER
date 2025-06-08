using System.Runtime.InteropServices;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public sealed class TablesHeapOptions
{
	public uint? Reserved1;

	public byte? MajorVersion;

	public byte? MinorVersion;

	public bool? UseENC;

	public bool? ForceBigColumns;

	public uint? ExtraData;

	public byte? Log2Rid;

	public bool? HasDeletedRows;

	public static TablesHeapOptions CreatePortablePdbV1_0()
	{
		return new TablesHeapOptions
		{
			Reserved1 = 0u,
			MajorVersion = (byte)2,
			MinorVersion = 0,
			UseENC = null,
			ExtraData = null,
			Log2Rid = null,
			HasDeletedRows = null
		};
	}
}
