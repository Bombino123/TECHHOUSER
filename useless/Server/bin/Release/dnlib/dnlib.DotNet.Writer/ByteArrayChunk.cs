using System;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.Writer;

public sealed class ByteArrayChunk : IReuseChunk, IChunk
{
	private readonly byte[] array;

	private readonly uint alignment;

	private FileOffset offset;

	private RVA rva;

	public FileOffset FileOffset => offset;

	public RVA RVA => rva;

	public byte[] Data => array;

	public ByteArrayChunk(byte[] array, uint alignment = 0u)
	{
		this.array = array ?? Array2.Empty<byte>();
		this.alignment = alignment;
	}

	bool IReuseChunk.CanReuse(RVA origRva, uint origSize)
	{
		return (uint)array.Length <= origSize;
	}

	public void SetOffset(FileOffset offset, RVA rva)
	{
		this.offset = offset;
		this.rva = rva;
	}

	public uint GetFileLength()
	{
		return (uint)array.Length;
	}

	public uint GetVirtualSize()
	{
		return GetFileLength();
	}

	public uint CalculateAlignment()
	{
		return alignment;
	}

	public void WriteTo(DataWriter writer)
	{
		writer.WriteBytes(array);
	}

	public override int GetHashCode()
	{
		return Utils.GetHashCode(array);
	}

	public override bool Equals(object obj)
	{
		if (obj is ByteArrayChunk byteArrayChunk)
		{
			return Utils.Equals(array, byteArrayChunk.array);
		}
		return false;
	}
}
