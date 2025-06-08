using System;
using System.IO;
using NAudio.Utils;

namespace NAudio.SoundFont;

internal class RiffChunk
{
	private string chunkID;

	private BinaryReader riffFile;

	public string ChunkID
	{
		get
		{
			return chunkID;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("ChunkID may not be null");
			}
			if (value.Length != 4)
			{
				throw new ArgumentException("ChunkID must be four characters");
			}
			chunkID = value;
		}
	}

	public uint ChunkSize { get; private set; }

	public long DataOffset { get; private set; }

	public static RiffChunk GetTopLevelChunk(BinaryReader file)
	{
		RiffChunk riffChunk = new RiffChunk(file);
		riffChunk.ReadChunk();
		return riffChunk;
	}

	private RiffChunk(BinaryReader file)
	{
		riffFile = file;
		chunkID = "????";
		ChunkSize = 0u;
		DataOffset = 0L;
	}

	public string ReadChunkID()
	{
		byte[] array = riffFile.ReadBytes(4);
		if (array.Length != 4)
		{
			throw new InvalidDataException("Couldn't read Chunk ID");
		}
		return ByteEncoding.Instance.GetString(array, 0, array.Length);
	}

	private void ReadChunk()
	{
		chunkID = ReadChunkID();
		ChunkSize = riffFile.ReadUInt32();
		DataOffset = riffFile.BaseStream.Position;
	}

	public RiffChunk GetNextSubChunk()
	{
		if (riffFile.BaseStream.Position + 8 < DataOffset + ChunkSize)
		{
			RiffChunk riffChunk = new RiffChunk(riffFile);
			riffChunk.ReadChunk();
			return riffChunk;
		}
		return null;
	}

	public byte[] GetData()
	{
		riffFile.BaseStream.Position = DataOffset;
		byte[] array = riffFile.ReadBytes((int)ChunkSize);
		if (array.Length != ChunkSize)
		{
			throw new InvalidDataException($"Couldn't read chunk's data Chunk: {this}, read {array.Length} bytes");
		}
		return array;
	}

	public string GetDataAsString()
	{
		byte[] data = GetData();
		if (data == null)
		{
			return null;
		}
		return ByteEncoding.Instance.GetString(data, 0, data.Length);
	}

	public T GetDataAsStructure<T>(StructureBuilder<T> s)
	{
		riffFile.BaseStream.Position = DataOffset;
		if (s.Length != ChunkSize)
		{
			throw new InvalidDataException($"Chunk size is: {ChunkSize} so can't read structure of: {s.Length}");
		}
		return s.Read(riffFile);
	}

	public T[] GetDataAsStructureArray<T>(StructureBuilder<T> s)
	{
		riffFile.BaseStream.Position = DataOffset;
		if (ChunkSize % s.Length != 0L)
		{
			throw new InvalidDataException($"Chunk size is: {ChunkSize} not a multiple of structure size: {s.Length}");
		}
		int num = (int)(ChunkSize / s.Length);
		T[] array = new T[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = s.Read(riffFile);
		}
		return array;
	}

	public override string ToString()
	{
		return $"RiffChunk ID: {ChunkID} Size: {ChunkSize} Data Offset: {DataOffset}";
	}
}
