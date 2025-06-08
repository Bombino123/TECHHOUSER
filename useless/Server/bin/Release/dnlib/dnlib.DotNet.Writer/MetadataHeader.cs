using System.Collections.Generic;
using System.Text;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.Writer;

public sealed class MetadataHeader : IChunk
{
	private IList<IHeap> heaps;

	private readonly MetadataHeaderOptions options;

	private uint length;

	private FileOffset offset;

	private RVA rva;

	public FileOffset FileOffset => offset;

	public RVA RVA => rva;

	public IList<IHeap> Heaps
	{
		get
		{
			return heaps;
		}
		set
		{
			heaps = value;
		}
	}

	public MetadataHeader()
		: this(null)
	{
	}

	public MetadataHeader(MetadataHeaderOptions options)
	{
		this.options = options ?? new MetadataHeaderOptions();
	}

	public void SetOffset(FileOffset offset, RVA rva)
	{
		this.offset = offset;
		this.rva = rva;
		length = 16u;
		length += (uint)GetVersionString().Length;
		length = Utils.AlignUp(length, 4u);
		length += 4u;
		IList<IHeap> list = heaps;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			IHeap heap = list[i];
			length += 8u;
			length += (uint)GetAsciizName(heap.Name).Length;
			length = Utils.AlignUp(length, 4u);
		}
	}

	public uint GetFileLength()
	{
		return length;
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
		writer.WriteUInt32(options.Signature ?? 1112167234);
		writer.WriteUInt16(options.MajorVersion ?? 1);
		writer.WriteUInt16(options.MinorVersion ?? 1);
		writer.WriteUInt32(options.Reserved1.GetValueOrDefault());
		byte[] versionString = GetVersionString();
		writer.WriteInt32(Utils.AlignUp(versionString.Length, 4u));
		writer.WriteBytes(versionString);
		writer.WriteZeroes(Utils.AlignUp(versionString.Length, 4u) - versionString.Length);
		writer.WriteByte((byte)options.StorageFlags.GetValueOrDefault());
		writer.WriteByte(options.Reserved2.GetValueOrDefault());
		IList<IHeap> list = heaps;
		writer.WriteUInt16((ushort)list.Count);
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			IHeap heap = list[i];
			writer.WriteUInt32(heap.FileOffset - offset);
			writer.WriteUInt32(heap.GetFileLength());
			writer.WriteBytes(versionString = GetAsciizName(heap.Name));
			if (versionString.Length > 32)
			{
				throw new ModuleWriterException("Heap name '" + heap.Name + "' is > 32 bytes");
			}
			writer.WriteZeroes(Utils.AlignUp(versionString.Length, 4u) - versionString.Length);
		}
	}

	private byte[] GetVersionString()
	{
		return Encoding.UTF8.GetBytes((options.VersionString ?? "v2.0.50727") + "\0");
	}

	private byte[] GetAsciizName(string s)
	{
		return Encoding.ASCII.GetBytes(s + "\0");
	}
}
