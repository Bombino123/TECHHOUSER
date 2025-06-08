using System.Collections.Generic;
using System.Text;
using dnlib.DotNet.MD;
using dnlib.IO;

namespace dnlib.DotNet.Pdb.Portable;

internal struct DocumentNameReader
{
	private const int MAX_NAME_LENGTH = 65536;

	private readonly Dictionary<uint, string> docNamePartDict;

	private readonly BlobStream blobStream;

	private readonly StringBuilder sb;

	private char[] prevSepChars;

	private int prevSepCharsLength;

	private byte[] prevSepCharBytes;

	private int prevSepCharBytesCount;

	public DocumentNameReader(BlobStream blobStream)
	{
		docNamePartDict = new Dictionary<uint, string>();
		this.blobStream = blobStream;
		sb = new StringBuilder();
		prevSepChars = new char[2];
		prevSepCharsLength = 0;
		prevSepCharBytes = new byte[3];
		prevSepCharBytesCount = 0;
	}

	public string ReadDocumentName(uint offset)
	{
		sb.Length = 0;
		if (!blobStream.TryCreateReader(offset, out var reader))
		{
			return string.Empty;
		}
		int charLength;
		char[] array = ReadSeparatorChar(ref reader, out charLength);
		bool flag = false;
		while (reader.Position < reader.Length)
		{
			if (flag)
			{
				sb.Append(array, 0, charLength);
			}
			flag = charLength != 1 || array[0] != '\0';
			string value = ReadDocumentNamePart(reader.ReadCompressedUInt32());
			sb.Append(value);
			if (sb.Length > 65536)
			{
				sb.Length = 65536;
				break;
			}
		}
		return sb.ToString();
	}

	private string ReadDocumentNamePart(uint offset)
	{
		if (docNamePartDict.TryGetValue(offset, out var value))
		{
			return value;
		}
		if (!blobStream.TryCreateReader(offset, out var reader))
		{
			return string.Empty;
		}
		value = reader.ReadUtf8String((int)reader.BytesLeft);
		docNamePartDict.Add(offset, value);
		return value;
	}

	private char[] ReadSeparatorChar(ref DataReader reader, out int charLength)
	{
		if (prevSepCharBytesCount != 0 && prevSepCharBytesCount <= reader.Length)
		{
			uint position = reader.Position;
			bool flag = true;
			for (int i = 0; i < prevSepCharBytesCount; i++)
			{
				if (i >= prevSepCharBytes.Length || reader.ReadByte() != prevSepCharBytes[i])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				charLength = prevSepCharsLength;
				return prevSepChars;
			}
			reader.Position = position;
		}
		Decoder decoder = Encoding.UTF8.GetDecoder();
		byte[] array = new byte[1];
		prevSepCharBytesCount = 0;
		int num = 0;
		while (true)
		{
			byte b = reader.ReadByte();
			prevSepCharBytesCount++;
			if (num == 0 && b == 0)
			{
				break;
			}
			if (num < prevSepCharBytes.Length)
			{
				prevSepCharBytes[num] = b;
			}
			array[0] = b;
			bool flush = reader.Position + 1 == reader.Length;
			decoder.Convert(array, 0, 1, prevSepChars, 0, prevSepChars.Length, flush, out var _, out prevSepCharsLength, out var _);
			if (prevSepCharsLength > 0)
			{
				break;
			}
			num++;
		}
		charLength = prevSepCharsLength;
		return prevSepChars;
	}
}
