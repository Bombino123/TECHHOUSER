using System;
using System.IO;
using System.Text;

namespace AntdUI.Svg.Text;

public class FontFamily
{
	public struct TT_OFFSET_TABLE
	{
		public ushort uMajorVersion;

		public ushort uMinorVersion;

		public ushort uNumOfTables;

		public ushort uSearchRange;

		public ushort uEntrySelector;

		public ushort uRangeShift;
	}

	public struct TT_TABLE_DIRECTORY
	{
		public byte[] szTag;

		public uint uCheckSum;

		public uint uOffset;

		public uint uLength;

		public void Initialize()
		{
			szTag = new byte[4];
		}
	}

	public struct TT_NAME_TABLE_HEADER
	{
		public ushort uFSelector;

		public ushort uNRCount;

		public ushort uStorageOffset;
	}

	public struct TT_NAME_RECORD
	{
		public ushort uPlatformID;

		public ushort uEncodingID;

		public ushort uLanguageID;

		public ushort uNameID;

		public ushort uStringLength;

		public ushort uStringOffset;
	}

	private string _fontName = string.Empty;

	private string _fontSubFamily = string.Empty;

	private string _fontPath = string.Empty;

	public string FontName
	{
		get
		{
			return _fontName;
		}
		set
		{
			_fontName = value;
		}
	}

	public string FontSubFamily
	{
		get
		{
			return _fontSubFamily;
		}
		set
		{
			_fontSubFamily = value;
		}
	}

	public string FontPath
	{
		get
		{
			return _fontPath;
		}
		set
		{
			_fontPath = value;
		}
	}

	private FontFamily(string fontName, string fontSubFamily, string fontPath)
	{
		_fontName = fontName;
		_fontSubFamily = fontSubFamily;
		_fontPath = fontPath;
	}

	public static FontFamily FromPath(string fontFilePath)
	{
		string fontName = string.Empty;
		string fontSubFamily = string.Empty;
		string name = "UTF-8";
		string empty = string.Empty;
		using FileStream fileStream = new FileStream(fontFilePath, FileMode.Open, FileAccess.Read);
		TT_OFFSET_TABLE tT_OFFSET_TABLE = default(TT_OFFSET_TABLE);
		tT_OFFSET_TABLE.uMajorVersion = ReadUShort(fileStream);
		tT_OFFSET_TABLE.uMinorVersion = ReadUShort(fileStream);
		tT_OFFSET_TABLE.uNumOfTables = ReadUShort(fileStream);
		tT_OFFSET_TABLE.uSearchRange = ReadUShort(fileStream);
		tT_OFFSET_TABLE.uEntrySelector = ReadUShort(fileStream);
		tT_OFFSET_TABLE.uRangeShift = ReadUShort(fileStream);
		TT_OFFSET_TABLE tT_OFFSET_TABLE2 = tT_OFFSET_TABLE;
		TT_TABLE_DIRECTORY tT_TABLE_DIRECTORY = default(TT_TABLE_DIRECTORY);
		bool flag = false;
		for (int i = 0; i <= tT_OFFSET_TABLE2.uNumOfTables; i++)
		{
			tT_TABLE_DIRECTORY = default(TT_TABLE_DIRECTORY);
			tT_TABLE_DIRECTORY.Initialize();
			fileStream.Read(tT_TABLE_DIRECTORY.szTag, 0, tT_TABLE_DIRECTORY.szTag.Length);
			tT_TABLE_DIRECTORY.uCheckSum = ReadULong(fileStream);
			tT_TABLE_DIRECTORY.uOffset = ReadULong(fileStream);
			tT_TABLE_DIRECTORY.uLength = ReadULong(fileStream);
			if (Encoding.GetEncoding(name).GetString(tT_TABLE_DIRECTORY.szTag).CompareTo("name") == 0)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return null;
		}
		fileStream.Seek(tT_TABLE_DIRECTORY.uOffset, SeekOrigin.Begin);
		TT_NAME_TABLE_HEADER tT_NAME_TABLE_HEADER = default(TT_NAME_TABLE_HEADER);
		tT_NAME_TABLE_HEADER.uFSelector = ReadUShort(fileStream);
		tT_NAME_TABLE_HEADER.uNRCount = ReadUShort(fileStream);
		tT_NAME_TABLE_HEADER.uStorageOffset = ReadUShort(fileStream);
		TT_NAME_TABLE_HEADER tT_NAME_TABLE_HEADER2 = tT_NAME_TABLE_HEADER;
		TT_NAME_RECORD tT_NAME_RECORD = default(TT_NAME_RECORD);
		for (int j = 0; j <= tT_NAME_TABLE_HEADER2.uNRCount; j++)
		{
			TT_NAME_RECORD tT_NAME_RECORD2 = default(TT_NAME_RECORD);
			tT_NAME_RECORD2.uPlatformID = ReadUShort(fileStream);
			tT_NAME_RECORD2.uEncodingID = ReadUShort(fileStream);
			tT_NAME_RECORD2.uLanguageID = ReadUShort(fileStream);
			tT_NAME_RECORD2.uNameID = ReadUShort(fileStream);
			tT_NAME_RECORD2.uStringLength = ReadUShort(fileStream);
			tT_NAME_RECORD2.uStringOffset = ReadUShort(fileStream);
			tT_NAME_RECORD = tT_NAME_RECORD2;
			if (tT_NAME_RECORD.uNameID > 2)
			{
				break;
			}
			long position = fileStream.Position;
			fileStream.Seek(tT_TABLE_DIRECTORY.uOffset + tT_NAME_RECORD.uStringOffset + tT_NAME_TABLE_HEADER2.uStorageOffset, SeekOrigin.Begin);
			byte[] array = new byte[tT_NAME_RECORD.uStringLength];
			fileStream.Read(array, 0, tT_NAME_RECORD.uStringLength);
			Encoding encoding = ((tT_NAME_RECORD.uEncodingID != 3 && tT_NAME_RECORD.uEncodingID != 1) ? Encoding.UTF8 : Encoding.BigEndianUnicode);
			empty = encoding.GetString(array);
			if (tT_NAME_RECORD.uNameID == 1)
			{
				fontName = empty;
			}
			if (tT_NAME_RECORD.uNameID == 2)
			{
				fontSubFamily = empty;
			}
			fileStream.Seek(position, SeekOrigin.Begin);
		}
		return new FontFamily(fontName, fontSubFamily, fontFilePath);
	}

	private static ushort ReadChar(FileStream fs, int characters)
	{
		byte[] array = new byte[Convert.ToByte(new string[characters].Length)];
		array = ReadAndSwap(fs, array.Length);
		return BitConverter.ToUInt16(array, 0);
	}

	private static ushort ReadByte(FileStream fs)
	{
		byte[] array = new byte[11];
		array = ReadAndSwap(fs, array.Length);
		return BitConverter.ToUInt16(array, 0);
	}

	private static ushort ReadUShort(FileStream fs)
	{
		byte[] array = new byte[2];
		array = ReadAndSwap(fs, array.Length);
		return BitConverter.ToUInt16(array, 0);
	}

	private static uint ReadULong(FileStream fs)
	{
		byte[] array = new byte[4];
		array = ReadAndSwap(fs, array.Length);
		return BitConverter.ToUInt32(array, 0);
	}

	private static byte[] ReadAndSwap(FileStream fs, int size)
	{
		byte[] array = new byte[size];
		fs.Read(array, 0, array.Length);
		Array.Reverse((Array)array);
		return array;
	}
}
