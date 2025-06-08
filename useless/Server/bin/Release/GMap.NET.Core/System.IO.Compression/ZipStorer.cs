using System.Collections.Generic;
using System.Text;

namespace System.IO.Compression;

public class ZipStorer : IDisposable
{
	public enum Compression : ushort
	{
		Store = 0,
		Deflate = 8
	}

	public struct ZipFileEntry
	{
		public Compression Method;

		public string FilenameInZip;

		public uint FileSize;

		public uint CompressedSize;

		public uint HeaderOffset;

		public uint FileOffset;

		public uint HeaderSize;

		public uint Crc32;

		public DateTime ModifyTime;

		public string Comment;

		public bool EncodeUTF8;

		public override string ToString()
		{
			return FilenameInZip;
		}
	}

	public bool EncodeUTF8;

	public bool ForceDeflating;

	private List<ZipFileEntry> Files = new List<ZipFileEntry>();

	private string _fileName;

	private Stream _zipFileStream;

	private string _comment = "";

	private byte[] _centralDirImage;

	private ushort _existingFiles;

	private FileAccess _access;

	private static uint[] _crcTable;

	private static readonly Encoding DefaultEncoding;

	static ZipStorer()
	{
		DefaultEncoding = Encoding.GetEncoding(437);
		_crcTable = new uint[256];
		for (int i = 0; i < _crcTable.Length; i++)
		{
			uint num = (uint)i;
			for (int j = 0; j < 8; j++)
			{
				num = (((num & 1) == 0) ? (num >> 1) : (0xEDB88320u ^ (num >> 1)));
			}
			_crcTable[i] = num;
		}
	}

	public static ZipStorer Create(string filename, string comment)
	{
		ZipStorer zipStorer = Create(new FileStream(filename, FileMode.Create, FileAccess.ReadWrite), comment);
		zipStorer._comment = comment;
		zipStorer._fileName = filename;
		return zipStorer;
	}

	public static ZipStorer Create(Stream stream, string comment)
	{
		return new ZipStorer
		{
			_comment = comment,
			_zipFileStream = stream,
			_access = FileAccess.Write
		};
	}

	public static ZipStorer Open(string filename, FileAccess access)
	{
		ZipStorer zipStorer = Open(new FileStream(filename, FileMode.Open, (access == FileAccess.Read) ? FileAccess.Read : FileAccess.ReadWrite), access);
		zipStorer._fileName = filename;
		return zipStorer;
	}

	public static ZipStorer Open(Stream stream, FileAccess access)
	{
		if (!stream.CanSeek && access != FileAccess.Read)
		{
			throw new InvalidOperationException("Stream cannot seek");
		}
		ZipStorer zipStorer = new ZipStorer();
		zipStorer._zipFileStream = stream;
		zipStorer._access = access;
		if (zipStorer.ReadFileInfo())
		{
			return zipStorer;
		}
		throw new InvalidDataException();
	}

	public void AddFile(Compression method, string pathname, string filenameInZip, string comment)
	{
		if (_access == FileAccess.Read)
		{
			throw new InvalidOperationException("Writing is not alowed");
		}
		FileStream fileStream = new FileStream(pathname, FileMode.Open, FileAccess.Read);
		AddStream(method, filenameInZip, fileStream, File.GetLastWriteTime(pathname), comment);
		fileStream.Close();
	}

	public void AddStream(Compression method, string filenameInZip, Stream source, DateTime modTime, string comment)
	{
		if (_access == FileAccess.Read)
		{
			throw new InvalidOperationException("Writing is not alowed");
		}
		if (Files.Count != 0)
		{
			ZipFileEntry zipFileEntry = Files[Files.Count - 1];
		}
		ZipFileEntry zfe = default(ZipFileEntry);
		zfe.Method = method;
		zfe.EncodeUTF8 = EncodeUTF8;
		zfe.FilenameInZip = NormalizedFilename(filenameInZip);
		zfe.Comment = ((comment == null) ? "" : comment);
		zfe.Crc32 = 0u;
		zfe.HeaderOffset = (uint)_zipFileStream.Position;
		zfe.ModifyTime = modTime;
		WriteLocalHeader(ref zfe);
		zfe.FileOffset = (uint)_zipFileStream.Position;
		Store(ref zfe, source);
		source.Close();
		UpdateCrcAndSizes(ref zfe);
		Files.Add(zfe);
	}

	public void Close()
	{
		if (_zipFileStream == null)
		{
			return;
		}
		if (_access != FileAccess.Read)
		{
			uint offset = (uint)_zipFileStream.Position;
			uint num = 0u;
			if (_centralDirImage != null)
			{
				_zipFileStream.Write(_centralDirImage, 0, _centralDirImage.Length);
			}
			for (int i = 0; i < Files.Count; i++)
			{
				long position = _zipFileStream.Position;
				WriteCentralDirRecord(Files[i]);
				num += (uint)(int)(_zipFileStream.Position - position);
			}
			if (_centralDirImage != null)
			{
				WriteEndRecord(num + (uint)_centralDirImage.Length, offset);
			}
			else
			{
				WriteEndRecord(num, offset);
			}
		}
		if (_zipFileStream != null)
		{
			_zipFileStream.Flush();
			_zipFileStream.Dispose();
			_zipFileStream = null;
		}
	}

	public List<ZipFileEntry> ReadCentralDir()
	{
		if (_centralDirImage == null)
		{
			throw new InvalidOperationException("Central directory currently does not exist");
		}
		List<ZipFileEntry> list = new List<ZipFileEntry>();
		ushort num2;
		ushort num3;
		ushort num4;
		for (int i = 0; i < _centralDirImage.Length && BitConverter.ToUInt32(_centralDirImage, i) == 33639248; i += 46 + num2 + num3 + num4)
		{
			bool num = (BitConverter.ToUInt16(_centralDirImage, i + 8) & 0x800) != 0;
			ushort method = BitConverter.ToUInt16(_centralDirImage, i + 10);
			uint dt = BitConverter.ToUInt32(_centralDirImage, i + 12);
			uint crc = BitConverter.ToUInt32(_centralDirImage, i + 16);
			uint compressedSize = BitConverter.ToUInt32(_centralDirImage, i + 20);
			uint fileSize = BitConverter.ToUInt32(_centralDirImage, i + 24);
			num2 = BitConverter.ToUInt16(_centralDirImage, i + 28);
			num3 = BitConverter.ToUInt16(_centralDirImage, i + 30);
			num4 = BitConverter.ToUInt16(_centralDirImage, i + 32);
			uint headerOffset = BitConverter.ToUInt32(_centralDirImage, i + 42);
			uint headerSize = (uint)(46 + num2 + num3 + num4);
			Encoding encoding = (num ? Encoding.UTF8 : DefaultEncoding);
			ZipFileEntry item = default(ZipFileEntry);
			item.Method = (Compression)method;
			item.FilenameInZip = encoding.GetString(_centralDirImage, i + 46, num2);
			item.FileOffset = GetFileOffset(headerOffset);
			item.FileSize = fileSize;
			item.CompressedSize = compressedSize;
			item.HeaderOffset = headerOffset;
			item.HeaderSize = headerSize;
			item.Crc32 = crc;
			item.ModifyTime = DosTimeToDateTime(dt);
			if (num4 > 0)
			{
				item.Comment = encoding.GetString(_centralDirImage, i + 46 + num2 + num3, num4);
			}
			list.Add(item);
		}
		return list;
	}

	public bool ExtractFile(ZipFileEntry zfe, string filename)
	{
		string directoryName = Path.GetDirectoryName(filename);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		if (Directory.Exists(filename))
		{
			return true;
		}
		Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write);
		bool num = ExtractFile(zfe, stream);
		if (num)
		{
			stream.Close();
		}
		File.SetCreationTime(filename, zfe.ModifyTime);
		File.SetLastWriteTime(filename, zfe.ModifyTime);
		return num;
	}

	public bool ExtractFile(ZipFileEntry zfe, Stream stream)
	{
		if (!stream.CanWrite)
		{
			throw new InvalidOperationException("Stream cannot be written");
		}
		byte[] array = new byte[4];
		_zipFileStream.Seek(zfe.HeaderOffset, SeekOrigin.Begin);
		_zipFileStream.Read(array, 0, 4);
		if (BitConverter.ToUInt32(array, 0) != 67324752)
		{
			return false;
		}
		Stream stream2;
		if (zfe.Method == Compression.Store)
		{
			stream2 = _zipFileStream;
		}
		else
		{
			if (zfe.Method != Compression.Deflate)
			{
				return false;
			}
			stream2 = new DeflateStream(_zipFileStream, CompressionMode.Decompress, leaveOpen: true);
		}
		byte[] array2 = new byte[16384];
		_zipFileStream.Seek(zfe.FileOffset, SeekOrigin.Begin);
		uint num = zfe.FileSize;
		while (num != 0)
		{
			int num2 = stream2.Read(array2, 0, (int)Math.Min(num, array2.Length));
			stream.Write(array2, 0, num2);
			num -= (uint)num2;
		}
		stream.Flush();
		if (zfe.Method == Compression.Deflate)
		{
			stream2.Dispose();
		}
		return true;
	}

	public static bool RemoveEntries(ref ZipStorer zip, List<ZipFileEntry> zfes)
	{
		if (!(zip._zipFileStream is FileStream))
		{
			throw new InvalidOperationException("RemoveEntries is allowed just over streams of type FileStream");
		}
		List<ZipFileEntry> list = zip.ReadCentralDir();
		string tempFileName = Path.GetTempFileName();
		string tempFileName2 = Path.GetTempFileName();
		try
		{
			ZipStorer zipStorer = Create(tempFileName, string.Empty);
			foreach (ZipFileEntry item in list)
			{
				if (!zfes.Contains(item) && zip.ExtractFile(item, tempFileName2))
				{
					zipStorer.AddFile(item.Method, tempFileName2, item.FilenameInZip, item.Comment);
				}
			}
			zip.Close();
			zipStorer.Close();
			File.Delete(zip._fileName);
			File.Move(tempFileName, zip._fileName);
			zip = Open(zip._fileName, zip._access);
		}
		catch
		{
			return false;
		}
		finally
		{
			if (File.Exists(tempFileName))
			{
				File.Delete(tempFileName);
			}
			if (File.Exists(tempFileName2))
			{
				File.Delete(tempFileName2);
			}
		}
		return true;
	}

	private uint GetFileOffset(uint headerOffset)
	{
		byte[] array = new byte[2];
		_zipFileStream.Seek(headerOffset + 26, SeekOrigin.Begin);
		_zipFileStream.Read(array, 0, 2);
		ushort num = BitConverter.ToUInt16(array, 0);
		_zipFileStream.Read(array, 0, 2);
		ushort num2 = BitConverter.ToUInt16(array, 0);
		return (uint)(30 + num + num2 + headerOffset);
	}

	private void WriteLocalHeader(ref ZipFileEntry zfe)
	{
		long position = _zipFileStream.Position;
		byte[] bytes = (zfe.EncodeUTF8 ? Encoding.UTF8 : DefaultEncoding).GetBytes(zfe.FilenameInZip);
		_zipFileStream.Write(new byte[6] { 80, 75, 3, 4, 20, 0 }, 0, 6);
		_zipFileStream.Write(BitConverter.GetBytes((ushort)(zfe.EncodeUTF8 ? 2048u : 0u)), 0, 2);
		_zipFileStream.Write(BitConverter.GetBytes((ushort)zfe.Method), 0, 2);
		_zipFileStream.Write(BitConverter.GetBytes(DateTimeToDosTime(zfe.ModifyTime)), 0, 4);
		_zipFileStream.Write(new byte[12], 0, 12);
		_zipFileStream.Write(BitConverter.GetBytes((ushort)bytes.Length), 0, 2);
		_zipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2);
		_zipFileStream.Write(bytes, 0, bytes.Length);
		zfe.HeaderSize = (uint)(_zipFileStream.Position - position);
	}

	private void WriteCentralDirRecord(ZipFileEntry zfe)
	{
		Encoding obj = (zfe.EncodeUTF8 ? Encoding.UTF8 : DefaultEncoding);
		byte[] bytes = obj.GetBytes(zfe.FilenameInZip);
		byte[] bytes2 = obj.GetBytes(zfe.Comment);
		_zipFileStream.Write(new byte[8] { 80, 75, 1, 2, 23, 11, 20, 0 }, 0, 8);
		_zipFileStream.Write(BitConverter.GetBytes((ushort)(zfe.EncodeUTF8 ? 2048u : 0u)), 0, 2);
		_zipFileStream.Write(BitConverter.GetBytes((ushort)zfe.Method), 0, 2);
		_zipFileStream.Write(BitConverter.GetBytes(DateTimeToDosTime(zfe.ModifyTime)), 0, 4);
		_zipFileStream.Write(BitConverter.GetBytes(zfe.Crc32), 0, 4);
		_zipFileStream.Write(BitConverter.GetBytes(zfe.CompressedSize), 0, 4);
		_zipFileStream.Write(BitConverter.GetBytes(zfe.FileSize), 0, 4);
		_zipFileStream.Write(BitConverter.GetBytes((ushort)bytes.Length), 0, 2);
		_zipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2);
		_zipFileStream.Write(BitConverter.GetBytes((ushort)bytes2.Length), 0, 2);
		_zipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2);
		_zipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2);
		_zipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2);
		_zipFileStream.Write(BitConverter.GetBytes((ushort)33024), 0, 2);
		_zipFileStream.Write(BitConverter.GetBytes(zfe.HeaderOffset), 0, 4);
		_zipFileStream.Write(bytes, 0, bytes.Length);
		_zipFileStream.Write(bytes2, 0, bytes2.Length);
	}

	private void WriteEndRecord(uint size, uint offset)
	{
		byte[] bytes = (EncodeUTF8 ? Encoding.UTF8 : DefaultEncoding).GetBytes(_comment);
		_zipFileStream.Write(new byte[8] { 80, 75, 5, 6, 0, 0, 0, 0 }, 0, 8);
		_zipFileStream.Write(BitConverter.GetBytes((ushort)Files.Count + _existingFiles), 0, 2);
		_zipFileStream.Write(BitConverter.GetBytes((ushort)Files.Count + _existingFiles), 0, 2);
		_zipFileStream.Write(BitConverter.GetBytes(size), 0, 4);
		_zipFileStream.Write(BitConverter.GetBytes(offset), 0, 4);
		_zipFileStream.Write(BitConverter.GetBytes((ushort)bytes.Length), 0, 2);
		_zipFileStream.Write(bytes, 0, bytes.Length);
	}

	private void Store(ref ZipFileEntry zfe, Stream source)
	{
		byte[] array = new byte[16384];
		uint num = 0u;
		long position = _zipFileStream.Position;
		long position2 = source.Position;
		Stream stream = ((zfe.Method != 0) ? new DeflateStream(_zipFileStream, CompressionMode.Compress, leaveOpen: true) : _zipFileStream);
		zfe.Crc32 = uint.MaxValue;
		int num2;
		do
		{
			num2 = source.Read(array, 0, array.Length);
			num += (uint)num2;
			if (num2 > 0)
			{
				stream.Write(array, 0, num2);
				for (uint num3 = 0u; num3 < num2; num3++)
				{
					zfe.Crc32 = _crcTable[(zfe.Crc32 ^ array[num3]) & 0xFF] ^ (zfe.Crc32 >> 8);
				}
			}
		}
		while (num2 == array.Length);
		stream.Flush();
		if (zfe.Method == Compression.Deflate)
		{
			stream.Dispose();
		}
		zfe.Crc32 ^= uint.MaxValue;
		zfe.FileSize = num;
		zfe.CompressedSize = (uint)(_zipFileStream.Position - position);
		if (zfe.Method == Compression.Deflate && !ForceDeflating && source.CanSeek && zfe.CompressedSize > zfe.FileSize)
		{
			zfe.Method = Compression.Store;
			_zipFileStream.Position = position;
			_zipFileStream.SetLength(position);
			source.Position = position2;
			Store(ref zfe, source);
		}
	}

	private uint DateTimeToDosTime(DateTime dt)
	{
		return (uint)((dt.Second / 2) | (dt.Minute << 5) | (dt.Hour << 11) | (dt.Day << 16) | (dt.Month << 21) | (dt.Year - 1980 << 25));
	}

	private DateTime DosTimeToDateTime(uint dt)
	{
		return new DateTime((int)((dt >> 25) + 1980), (int)((dt >> 21) & 0xF), (int)((dt >> 16) & 0x1F), (int)((dt >> 11) & 0x1F), (int)((dt >> 5) & 0x3F), (int)((dt & 0x1F) * 2));
	}

	private void UpdateCrcAndSizes(ref ZipFileEntry zfe)
	{
		long position = _zipFileStream.Position;
		_zipFileStream.Position = zfe.HeaderOffset + 8;
		_zipFileStream.Write(BitConverter.GetBytes((ushort)zfe.Method), 0, 2);
		_zipFileStream.Position = zfe.HeaderOffset + 14;
		_zipFileStream.Write(BitConverter.GetBytes(zfe.Crc32), 0, 4);
		_zipFileStream.Write(BitConverter.GetBytes(zfe.CompressedSize), 0, 4);
		_zipFileStream.Write(BitConverter.GetBytes(zfe.FileSize), 0, 4);
		_zipFileStream.Position = position;
	}

	private string NormalizedFilename(string filename)
	{
		filename = filename.Replace('\\', '/');
		int num = filename.IndexOf(':');
		if (num >= 0)
		{
			filename = filename.Remove(0, num + 1);
		}
		return filename.Trim(new char[1] { '/' });
	}

	private bool ReadFileInfo()
	{
		if (_zipFileStream.Length < 22)
		{
			return false;
		}
		try
		{
			_zipFileStream.Seek(-17L, SeekOrigin.End);
			BinaryReader binaryReader = new BinaryReader(_zipFileStream);
			do
			{
				_zipFileStream.Seek(-5L, SeekOrigin.Current);
				if (binaryReader.ReadUInt32() == 101010256)
				{
					_zipFileStream.Seek(6L, SeekOrigin.Current);
					ushort existingFiles = binaryReader.ReadUInt16();
					int num = binaryReader.ReadInt32();
					uint num2 = binaryReader.ReadUInt32();
					ushort num3 = binaryReader.ReadUInt16();
					if (_zipFileStream.Position + num3 != _zipFileStream.Length)
					{
						return false;
					}
					_existingFiles = existingFiles;
					_centralDirImage = new byte[num];
					_zipFileStream.Seek(num2, SeekOrigin.Begin);
					_zipFileStream.Read(_centralDirImage, 0, num);
					_zipFileStream.Seek(num2, SeekOrigin.Begin);
					return true;
				}
			}
			while (_zipFileStream.Position > 0);
		}
		catch
		{
		}
		return false;
	}

	public void Dispose()
	{
		Close();
	}
}
