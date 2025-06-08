using System.Runtime.InteropServices;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public sealed class ImportAddressTable : IChunk
{
	private readonly bool is64bit;

	private FileOffset offset;

	private RVA rva;

	public ImportDirectory ImportDirectory { get; set; }

	public FileOffset FileOffset => offset;

	public RVA RVA => rva;

	internal bool Enable { get; set; }

	public ImportAddressTable(bool is64bit)
	{
		this.is64bit = is64bit;
	}

	public void SetOffset(FileOffset offset, RVA rva)
	{
		this.offset = offset;
		this.rva = rva;
	}

	public uint GetFileLength()
	{
		if (!Enable)
		{
			return 0u;
		}
		if (!is64bit)
		{
			return 8u;
		}
		return 16u;
	}

	public uint GetVirtualSize()
	{
		return GetFileLength();
	}

	public uint CalculateAlignment()
	{
		return 0u;
	}

	public void WriteTo(DataWriter writer)
	{
		if (Enable)
		{
			if (is64bit)
			{
				writer.WriteUInt64((ulong)ImportDirectory.CorXxxMainRVA);
				writer.WriteUInt64(0uL);
			}
			else
			{
				writer.WriteUInt32((uint)ImportDirectory.CorXxxMainRVA);
				writer.WriteInt32(0);
			}
		}
	}
}
