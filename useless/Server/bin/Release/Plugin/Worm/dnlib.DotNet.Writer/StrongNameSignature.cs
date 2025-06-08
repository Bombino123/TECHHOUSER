using System.Runtime.InteropServices;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public sealed class StrongNameSignature : IReuseChunk, IChunk
{
	private FileOffset offset;

	private RVA rva;

	private int size;

	public FileOffset FileOffset => offset;

	public RVA RVA => rva;

	public StrongNameSignature(int size)
	{
		this.size = size;
	}

	bool IReuseChunk.CanReuse(RVA origRva, uint origSize)
	{
		return (uint)size <= origSize;
	}

	public void SetOffset(FileOffset offset, RVA rva)
	{
		this.offset = offset;
		this.rva = rva;
	}

	public uint GetFileLength()
	{
		return (uint)size;
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
		writer.WriteZeroes(size);
	}
}
