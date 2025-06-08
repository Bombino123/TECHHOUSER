using System.Text;

namespace dnlib.DotNet.Writer;

public sealed class PESection : ChunkList<IChunk>
{
	private string name;

	private uint characteristics;

	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	public uint Characteristics
	{
		get
		{
			return characteristics;
		}
		set
		{
			characteristics = value;
		}
	}

	public bool IsCode => (characteristics & 0x20) != 0;

	public bool IsInitializedData => (characteristics & 0x40) != 0;

	public bool IsUninitializedData => (characteristics & 0x80) != 0;

	public PESection(string name, uint characteristics)
	{
		this.name = name;
		this.characteristics = characteristics;
	}

	public uint WriteHeaderTo(DataWriter writer, uint fileAlignment, uint sectionAlignment, uint rva)
	{
		uint num = GetVirtualSize();
		uint fileLength = GetFileLength();
		uint result = Utils.AlignUp(num, sectionAlignment);
		uint value = Utils.AlignUp(fileLength, fileAlignment);
		uint fileOffset = (uint)base.FileOffset;
		writer.WriteBytes(Encoding.UTF8.GetBytes(Name + "\0\0\0\0\0\0\0\0"), 0, 8);
		writer.WriteUInt32(num);
		writer.WriteUInt32(rva);
		writer.WriteUInt32(value);
		writer.WriteUInt32(fileOffset);
		writer.WriteInt32(0);
		writer.WriteInt32(0);
		writer.WriteUInt16(0);
		writer.WriteUInt16(0);
		writer.WriteUInt32(Characteristics);
		return result;
	}
}
