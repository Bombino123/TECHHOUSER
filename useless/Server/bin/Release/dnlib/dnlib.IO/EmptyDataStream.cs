using System.Text;

namespace dnlib.IO;

internal sealed class EmptyDataStream : DataStream
{
	public static readonly DataStream Instance = new EmptyDataStream();

	private EmptyDataStream()
	{
	}

	public unsafe override void ReadBytes(uint offset, void* destination, int length)
	{
		for (int i = 0; i < length; i++)
		{
			*(sbyte*)destination = 0;
		}
	}

	public override void ReadBytes(uint offset, byte[] destination, int destinationIndex, int length)
	{
		for (int i = 0; i < length; i++)
		{
			destination[destinationIndex + i] = 0;
		}
	}

	public override byte ReadByte(uint offset)
	{
		return 0;
	}

	public override ushort ReadUInt16(uint offset)
	{
		return 0;
	}

	public override uint ReadUInt32(uint offset)
	{
		return 0u;
	}

	public override ulong ReadUInt64(uint offset)
	{
		return 0uL;
	}

	public override float ReadSingle(uint offset)
	{
		return 0f;
	}

	public override double ReadDouble(uint offset)
	{
		return 0.0;
	}

	public override string ReadUtf16String(uint offset, int chars)
	{
		return string.Empty;
	}

	public override string ReadString(uint offset, int length, Encoding encoding)
	{
		return string.Empty;
	}

	public override bool TryGetOffsetOf(uint offset, uint endOffset, byte value, out uint valueOffset)
	{
		valueOffset = 0u;
		return false;
	}
}
