using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using dnlib.DotNet.MD;
using dnlib.IO;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public sealed class BlobHeap : HeapBase, IOffsetHeap<byte[]>
{
	private readonly Dictionary<byte[], uint> cachedDict = new Dictionary<byte[], uint>(ByteArrayEqualityComparer.Instance);

	private readonly List<byte[]> cached = new List<byte[]>();

	private uint nextOffset = 1u;

	private byte[] originalData;

	private Dictionary<uint, byte[]> userRawData;

	public override string Name => "#Blob";

	public void Populate(BlobStream blobStream)
	{
		if (isReadOnly)
		{
			throw new ModuleWriterException("Trying to modify #Blob when it's read-only");
		}
		if (originalData != null)
		{
			throw new InvalidOperationException("Can't call method twice");
		}
		if (nextOffset != 1)
		{
			throw new InvalidOperationException("Add() has already been called");
		}
		if (blobStream != null && blobStream.StreamLength != 0)
		{
			DataReader reader = blobStream.CreateReader();
			originalData = reader.ToArray();
			nextOffset = (uint)originalData.Length;
			Populate(ref reader);
		}
	}

	private void Populate(ref DataReader reader)
	{
		reader.Position = 1u;
		while (reader.Position < reader.Length)
		{
			uint position = reader.Position;
			if (!reader.TryReadCompressedUInt32(out var value))
			{
				if (position == reader.Position)
				{
					reader.Position++;
				}
			}
			else if (value != 0 && (ulong)((long)reader.Position + (long)value) <= (ulong)reader.Length)
			{
				byte[] key = reader.ReadBytes((int)value);
				if (!cachedDict.ContainsKey(key))
				{
					cachedDict[key] = position;
				}
			}
		}
	}

	public uint Add(byte[] data)
	{
		if (isReadOnly)
		{
			throw new ModuleWriterException("Trying to modify #Blob when it's read-only");
		}
		if (data == null || data.Length == 0)
		{
			return 0u;
		}
		if (cachedDict.TryGetValue(data, out var value))
		{
			return value;
		}
		return AddToCache(data);
	}

	public uint Create(byte[] data)
	{
		if (isReadOnly)
		{
			throw new ModuleWriterException("Trying to modify #Blob when it's read-only");
		}
		return AddToCache(data ?? Array2.Empty<byte>());
	}

	private uint AddToCache(byte[] data)
	{
		cached.Add(data);
		uint result = (cachedDict[data] = nextOffset);
		nextOffset += (uint)GetRawDataSize(data);
		return result;
	}

	public override uint GetRawLength()
	{
		return nextOffset;
	}

	protected override void WriteToImpl(DataWriter writer)
	{
		if (originalData != null)
		{
			writer.WriteBytes(originalData);
		}
		else
		{
			writer.WriteByte(0);
		}
		uint num = ((originalData == null) ? 1u : ((uint)originalData.Length));
		foreach (byte[] item in cached)
		{
			int rawDataSize = GetRawDataSize(item);
			if (userRawData != null && userRawData.TryGetValue(num, out var value))
			{
				if (value.Length != rawDataSize)
				{
					throw new InvalidOperationException("Invalid length of raw data");
				}
				writer.WriteBytes(value);
			}
			else
			{
				writer.WriteCompressedUInt32((uint)item.Length);
				writer.WriteBytes(item);
			}
			num += (uint)rawDataSize;
		}
	}

	public int GetRawDataSize(byte[] data)
	{
		return DataWriter.GetCompressedUInt32Length((uint)data.Length) + data.Length;
	}

	public void SetRawData(uint offset, byte[] rawData)
	{
		if (userRawData == null)
		{
			userRawData = new Dictionary<uint, byte[]>();
		}
		userRawData[offset] = rawData ?? throw new ArgumentNullException("rawData");
	}

	public IEnumerable<KeyValuePair<uint, byte[]>> GetAllRawData()
	{
		MemoryStream memStream = new MemoryStream();
		DataWriter writer = new DataWriter(memStream);
		uint offset = ((originalData == null) ? 1u : ((uint)originalData.Length));
		foreach (byte[] item in cached)
		{
			memStream.Position = 0L;
			memStream.SetLength(0L);
			writer.WriteCompressedUInt32((uint)item.Length);
			writer.WriteBytes(item);
			yield return new KeyValuePair<uint, byte[]>(offset, memStream.ToArray());
			offset += (uint)(int)memStream.Length;
		}
	}
}
