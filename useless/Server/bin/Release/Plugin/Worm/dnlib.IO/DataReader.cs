using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using dnlib.DotNet.Writer;

namespace dnlib.IO;

[DebuggerDisplay("{StartOffset,h}-{EndOffset,h} Length={Length} BytesLeft={BytesLeft}")]
[ComVisible(true)]
public struct DataReader
{
	private readonly DataStream stream;

	private readonly uint startOffset;

	private readonly uint endOffset;

	private uint currentOffset;

	public readonly uint StartOffset => startOffset;

	public readonly uint EndOffset => endOffset;

	public readonly uint Length => endOffset - startOffset;

	public uint CurrentOffset
	{
		readonly get
		{
			return currentOffset;
		}
		set
		{
			if (value < startOffset || value > endOffset)
			{
				ThrowDataReaderException("Invalid new CurrentOffset");
			}
			currentOffset = value;
		}
	}

	public uint Position
	{
		readonly get
		{
			return currentOffset - startOffset;
		}
		set
		{
			if (value > Length)
			{
				ThrowDataReaderException("Invalid new Position");
			}
			currentOffset = startOffset + value;
		}
	}

	public readonly uint BytesLeft => endOffset - currentOffset;

	public DataReader(DataStream stream, uint offset, uint length)
	{
		this.stream = stream;
		startOffset = offset;
		endOffset = offset + length;
		currentOffset = offset;
	}

	[Conditional("DEBUG")]
	private readonly void VerifyState()
	{
	}

	private static void ThrowNoMoreBytesLeft()
	{
		throw new DataReaderException("There's not enough bytes left to read");
	}

	private static void ThrowDataReaderException(string message)
	{
		throw new DataReaderException(message);
	}

	private static void ThrowInvalidOperationException()
	{
		throw new InvalidOperationException();
	}

	private static void ThrowArgumentNullException(string paramName)
	{
		throw new ArgumentNullException(paramName);
	}

	private static void ThrowInvalidArgument(string paramName)
	{
		throw new DataReaderException("Invalid argument value");
	}

	public void Reset()
	{
		currentOffset = startOffset;
	}

	public readonly DataReader Slice(uint start, uint length)
	{
		if ((ulong)((long)start + (long)length) > (ulong)Length)
		{
			ThrowInvalidArgument("length");
		}
		return new DataReader(stream, startOffset + start, length);
	}

	public readonly DataReader Slice(uint start)
	{
		if (start > Length)
		{
			ThrowInvalidArgument("start");
		}
		return Slice(start, Length - start);
	}

	public readonly DataReader Slice(int start, int length)
	{
		if (start < 0)
		{
			ThrowInvalidArgument("start");
		}
		if (length < 0)
		{
			ThrowInvalidArgument("length");
		}
		return Slice((uint)start, (uint)length);
	}

	public readonly DataReader Slice(int start)
	{
		if (start < 0)
		{
			ThrowInvalidArgument("start");
		}
		if ((uint)start > Length)
		{
			ThrowInvalidArgument("start");
		}
		return Slice((uint)start, Length - (uint)start);
	}

	public readonly bool CanRead(int length)
	{
		if (length >= 0)
		{
			return (uint)length <= BytesLeft;
		}
		return false;
	}

	public readonly bool CanRead(uint length)
	{
		return length <= BytesLeft;
	}

	public bool ReadBoolean()
	{
		uint num = currentOffset;
		if (num == endOffset)
		{
			ThrowNoMoreBytesLeft();
		}
		bool result = stream.ReadBoolean(num);
		currentOffset = num + 1;
		return result;
	}

	public char ReadChar()
	{
		uint num = currentOffset;
		if (endOffset - num < 2)
		{
			ThrowNoMoreBytesLeft();
		}
		char result = stream.ReadChar(num);
		currentOffset = num + 2;
		return result;
	}

	public sbyte ReadSByte()
	{
		uint num = currentOffset;
		if (num == endOffset)
		{
			ThrowNoMoreBytesLeft();
		}
		sbyte result = stream.ReadSByte(num);
		currentOffset = num + 1;
		return result;
	}

	public byte ReadByte()
	{
		uint num = currentOffset;
		if (num == endOffset)
		{
			ThrowNoMoreBytesLeft();
		}
		byte result = stream.ReadByte(num);
		currentOffset = num + 1;
		return result;
	}

	public short ReadInt16()
	{
		uint num = currentOffset;
		if (endOffset - num < 2)
		{
			ThrowNoMoreBytesLeft();
		}
		short result = stream.ReadInt16(num);
		currentOffset = num + 2;
		return result;
	}

	public ushort ReadUInt16()
	{
		uint num = currentOffset;
		if (endOffset - num < 2)
		{
			ThrowNoMoreBytesLeft();
		}
		ushort result = stream.ReadUInt16(num);
		currentOffset = num + 2;
		return result;
	}

	public int ReadInt32()
	{
		uint num = currentOffset;
		if (endOffset - num < 4)
		{
			ThrowNoMoreBytesLeft();
		}
		int result = stream.ReadInt32(num);
		currentOffset = num + 4;
		return result;
	}

	public uint ReadUInt32()
	{
		uint num = currentOffset;
		if (endOffset - num < 4)
		{
			ThrowNoMoreBytesLeft();
		}
		uint result = stream.ReadUInt32(num);
		currentOffset = num + 4;
		return result;
	}

	internal byte Unsafe_ReadByte()
	{
		uint num = currentOffset;
		byte result = stream.ReadByte(num);
		currentOffset = num + 1;
		return result;
	}

	internal ushort Unsafe_ReadUInt16()
	{
		uint num = currentOffset;
		ushort result = stream.ReadUInt16(num);
		currentOffset = num + 2;
		return result;
	}

	internal uint Unsafe_ReadUInt32()
	{
		uint num = currentOffset;
		uint result = stream.ReadUInt32(num);
		currentOffset = num + 4;
		return result;
	}

	public long ReadInt64()
	{
		uint num = currentOffset;
		if (endOffset - num < 8)
		{
			ThrowNoMoreBytesLeft();
		}
		long result = stream.ReadInt64(num);
		currentOffset = num + 8;
		return result;
	}

	public ulong ReadUInt64()
	{
		uint num = currentOffset;
		if (endOffset - num < 8)
		{
			ThrowNoMoreBytesLeft();
		}
		ulong result = stream.ReadUInt64(num);
		currentOffset = num + 8;
		return result;
	}

	public float ReadSingle()
	{
		uint num = currentOffset;
		if (endOffset - num < 4)
		{
			ThrowNoMoreBytesLeft();
		}
		float result = stream.ReadSingle(num);
		currentOffset = num + 4;
		return result;
	}

	public double ReadDouble()
	{
		uint num = currentOffset;
		if (endOffset - num < 8)
		{
			ThrowNoMoreBytesLeft();
		}
		double result = stream.ReadDouble(num);
		currentOffset = num + 8;
		return result;
	}

	public Guid ReadGuid()
	{
		uint num = currentOffset;
		if (endOffset - num < 16)
		{
			ThrowNoMoreBytesLeft();
		}
		Guid result = stream.ReadGuid(num);
		currentOffset = num + 16;
		return result;
	}

	public decimal ReadDecimal()
	{
		uint num = currentOffset;
		if (endOffset - num < 16)
		{
			ThrowNoMoreBytesLeft();
		}
		decimal result = stream.ReadDecimal(num);
		currentOffset = num + 16;
		return result;
	}

	public string ReadUtf16String(int chars)
	{
		if (chars < 0)
		{
			ThrowInvalidArgument("chars");
		}
		if (chars == 0)
		{
			return string.Empty;
		}
		uint num = (uint)(chars * 2);
		uint num2 = currentOffset;
		if (endOffset - num2 < num)
		{
			ThrowNoMoreBytesLeft();
		}
		string result = ((num == 0) ? string.Empty : stream.ReadUtf16String(num2, chars));
		currentOffset = num2 + num;
		return result;
	}

	public unsafe void ReadBytes(void* destination, int length)
	{
		if (destination == null && length != 0)
		{
			ThrowArgumentNullException("destination");
		}
		if (length < 0)
		{
			ThrowInvalidArgument("length");
		}
		if (length != 0)
		{
			uint num = currentOffset;
			if (endOffset - num < (uint)length)
			{
				ThrowNoMoreBytesLeft();
			}
			stream.ReadBytes(num, destination, length);
			currentOffset = num + (uint)length;
		}
	}

	public void ReadBytes(byte[] destination, int destinationIndex, int length)
	{
		if (destination == null)
		{
			ThrowArgumentNullException("destination");
		}
		if (destinationIndex < 0)
		{
			ThrowInvalidArgument("destinationIndex");
		}
		if (length < 0)
		{
			ThrowInvalidArgument("length");
		}
		if (length != 0)
		{
			uint num = currentOffset;
			if (endOffset - num < (uint)length)
			{
				ThrowNoMoreBytesLeft();
			}
			stream.ReadBytes(num, destination, destinationIndex, length);
			currentOffset = num + (uint)length;
		}
	}

	public byte[] ReadBytes(int length)
	{
		if (length < 0)
		{
			ThrowInvalidArgument("length");
		}
		if (length == 0)
		{
			return Array2.Empty<byte>();
		}
		byte[] array = new byte[length];
		ReadBytes(array, 0, length);
		return array;
	}

	public bool TryReadCompressedUInt32(out uint value)
	{
		uint num = currentOffset;
		uint num2 = endOffset - num;
		if (num2 == 0)
		{
			value = 0u;
			return false;
		}
		DataStream dataStream = stream;
		byte b = dataStream.ReadByte(num++);
		if ((b & 0x80) == 0)
		{
			value = b;
			currentOffset = num;
			return true;
		}
		if ((b & 0xC0) == 128)
		{
			if (num2 < 2)
			{
				value = 0u;
				return false;
			}
			value = (uint)(((b & 0x3F) << 8) | dataStream.ReadByte(num++));
			currentOffset = num;
			return true;
		}
		if (num2 < 4)
		{
			value = 0u;
			return false;
		}
		value = (uint)(((b & 0x1F) << 24) | (dataStream.ReadByte(num++) << 16) | (dataStream.ReadByte(num++) << 8) | dataStream.ReadByte(num++));
		currentOffset = num;
		return true;
	}

	public uint ReadCompressedUInt32()
	{
		if (!TryReadCompressedUInt32(out var value))
		{
			ThrowNoMoreBytesLeft();
		}
		return value;
	}

	public bool TryReadCompressedInt32(out int value)
	{
		uint num = currentOffset;
		uint num2 = endOffset - num;
		if (num2 == 0)
		{
			value = 0;
			return false;
		}
		DataStream dataStream = stream;
		byte b = dataStream.ReadByte(num++);
		if ((b & 0x80) == 0)
		{
			if (((uint)b & (true ? 1u : 0u)) != 0)
			{
				value = -64 | (b >> 1);
			}
			else
			{
				value = b >> 1;
			}
			currentOffset = num;
			return true;
		}
		if ((b & 0xC0) == 128)
		{
			if (num2 < 2)
			{
				value = 0;
				return false;
			}
			uint num3 = (uint)(((b & 0x3F) << 8) | dataStream.ReadByte(num++));
			if ((num3 & (true ? 1u : 0u)) != 0)
			{
				value = -8192 | (int)(num3 >> 1);
			}
			else
			{
				value = (int)(num3 >> 1);
			}
			currentOffset = num;
			return true;
		}
		if ((b & 0xE0) == 192)
		{
			if (num2 < 4)
			{
				value = 0;
				return false;
			}
			uint num4 = (uint)(((b & 0x1F) << 24) | (dataStream.ReadByte(num++) << 16) | (dataStream.ReadByte(num++) << 8) | dataStream.ReadByte(num++));
			if ((num4 & (true ? 1u : 0u)) != 0)
			{
				value = -268435456 | (int)(num4 >> 1);
			}
			else
			{
				value = (int)(num4 >> 1);
			}
			currentOffset = num;
			return true;
		}
		value = 0;
		return false;
	}

	public int ReadCompressedInt32()
	{
		if (!TryReadCompressedInt32(out var value))
		{
			ThrowNoMoreBytesLeft();
		}
		return value;
	}

	public uint Read7BitEncodedUInt32()
	{
		uint num = 0u;
		int num2 = 0;
		for (int i = 0; i < 5; i++)
		{
			byte b = ReadByte();
			num |= (uint)((b & 0x7F) << num2);
			if ((b & 0x80) == 0)
			{
				return num;
			}
			num2 += 7;
		}
		ThrowDataReaderException("Invalid encoded UInt32");
		return 0u;
	}

	public int Read7BitEncodedInt32()
	{
		return (int)Read7BitEncodedUInt32();
	}

	public string ReadSerializedString()
	{
		return ReadSerializedString(Encoding.UTF8);
	}

	public string ReadSerializedString(Encoding encoding)
	{
		if (encoding == null)
		{
			ThrowArgumentNullException("encoding");
		}
		int num = Read7BitEncodedInt32();
		if (num < 0)
		{
			ThrowNoMoreBytesLeft();
		}
		if (num == 0)
		{
			return string.Empty;
		}
		return ReadString(num, encoding);
	}

	public readonly byte[] ToArray()
	{
		int length = (int)Length;
		if (length < 0)
		{
			ThrowInvalidOperationException();
		}
		if (length == 0)
		{
			return Array2.Empty<byte>();
		}
		byte[] array = new byte[length];
		stream.ReadBytes(startOffset, array, 0, array.Length);
		return array;
	}

	public byte[] ReadRemainingBytes()
	{
		int bytesLeft = (int)BytesLeft;
		if (bytesLeft < 0)
		{
			ThrowInvalidOperationException();
		}
		return ReadBytes(bytesLeft);
	}

	public byte[] TryReadBytesUntil(byte value)
	{
		uint num = currentOffset;
		uint num2 = endOffset;
		if (num == num2)
		{
			return null;
		}
		if (!stream.TryGetOffsetOf(num, num2, value, out var valueOffset))
		{
			return null;
		}
		int num3 = (int)(valueOffset - num);
		if (num3 < 0)
		{
			return null;
		}
		return ReadBytes(num3);
	}

	public string TryReadZeroTerminatedUtf8String()
	{
		return TryReadZeroTerminatedString(Encoding.UTF8);
	}

	public string TryReadZeroTerminatedString(Encoding encoding)
	{
		if (encoding == null)
		{
			ThrowArgumentNullException("encoding");
		}
		uint num = currentOffset;
		uint num2 = endOffset;
		if (num == num2)
		{
			return null;
		}
		if (!stream.TryGetOffsetOf(num, num2, 0, out var valueOffset))
		{
			return null;
		}
		int num3 = (int)(valueOffset - num);
		if (num3 < 0)
		{
			return null;
		}
		string result = ((num3 == 0) ? string.Empty : stream.ReadString(num, num3, encoding));
		currentOffset = valueOffset + 1;
		return result;
	}

	public string ReadUtf8String(int byteCount)
	{
		return ReadString(byteCount, Encoding.UTF8);
	}

	public string ReadString(int byteCount, Encoding encoding)
	{
		if (byteCount < 0)
		{
			ThrowInvalidArgument("byteCount");
		}
		if (encoding == null)
		{
			ThrowArgumentNullException("encoding");
		}
		if (byteCount == 0)
		{
			return string.Empty;
		}
		if ((uint)byteCount > Length)
		{
			ThrowInvalidArgument("byteCount");
		}
		uint num = currentOffset;
		string result = stream.ReadString(num, byteCount, encoding);
		currentOffset = num + (uint)byteCount;
		return result;
	}

	public readonly Stream AsStream()
	{
		return new DataReaderStream(in this);
	}

	private readonly byte[] AllocTempBuffer()
	{
		return new byte[Math.Min(8192u, BytesLeft)];
	}

	public void CopyTo(DataWriter destination)
	{
		if (destination == null)
		{
			ThrowArgumentNullException("destination");
		}
		if (Position < Length)
		{
			CopyTo(destination.InternalStream, AllocTempBuffer());
		}
	}

	public void CopyTo(DataWriter destination, byte[] dataBuffer)
	{
		if (destination == null)
		{
			ThrowArgumentNullException("destination");
		}
		CopyTo(destination.InternalStream, dataBuffer);
	}

	public void CopyTo(BinaryWriter destination)
	{
		if (destination == null)
		{
			ThrowArgumentNullException("destination");
		}
		if (Position < Length)
		{
			CopyTo(destination.BaseStream, AllocTempBuffer());
		}
	}

	public void CopyTo(BinaryWriter destination, byte[] dataBuffer)
	{
		if (destination == null)
		{
			ThrowArgumentNullException("destination");
		}
		CopyTo(destination.BaseStream, dataBuffer);
	}

	public void CopyTo(Stream destination)
	{
		if (destination == null)
		{
			ThrowArgumentNullException("destination");
		}
		if (Position < Length)
		{
			CopyTo(destination, AllocTempBuffer());
		}
	}

	public void CopyTo(Stream destination, byte[] dataBuffer)
	{
		if (destination == null)
		{
			ThrowArgumentNullException("destination");
		}
		if (dataBuffer == null)
		{
			ThrowArgumentNullException("dataBuffer");
		}
		if (Position < Length)
		{
			if (dataBuffer.Length == 0)
			{
				ThrowInvalidArgument("dataBuffer");
			}
			uint num = BytesLeft;
			while (num != 0)
			{
				int num2 = (int)Math.Min((uint)dataBuffer.Length, num);
				num -= (uint)num2;
				ReadBytes(dataBuffer, 0, num2);
				destination.Write(dataBuffer, 0, num2);
			}
		}
	}
}
