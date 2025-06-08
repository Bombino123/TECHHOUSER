using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using dnlib.DotNet.MD;
using dnlib.IO;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public sealed class USHeap : HeapBase, IOffsetHeap<string>
{
	private readonly Dictionary<string, uint> cachedDict = new Dictionary<string, uint>(StringComparer.Ordinal);

	private readonly List<string> cached = new List<string>();

	private uint nextOffset = 1u;

	private byte[] originalData;

	private Dictionary<uint, byte[]> userRawData;

	public override string Name => "#US";

	public void Populate(USStream usStream)
	{
		if (originalData != null)
		{
			throw new InvalidOperationException("Can't call method twice");
		}
		if (nextOffset != 1)
		{
			throw new InvalidOperationException("Add() has already been called");
		}
		if (usStream != null && usStream.StreamLength != 0)
		{
			DataReader reader = usStream.CreateReader();
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
				int chars = (int)value / 2;
				string key = reader.ReadUtf16String(chars);
				if ((value & (true ? 1u : 0u)) != 0)
				{
					reader.ReadByte();
				}
				if (!cachedDict.ContainsKey(key))
				{
					cachedDict[key] = position;
				}
			}
		}
	}

	public uint Add(string s)
	{
		if (isReadOnly)
		{
			throw new ModuleWriterException("Trying to modify #US when it's read-only");
		}
		if (s == null)
		{
			s = string.Empty;
		}
		if (cachedDict.TryGetValue(s, out var value))
		{
			return value;
		}
		return AddToCache(s);
	}

	public uint Create(string s)
	{
		if (isReadOnly)
		{
			throw new ModuleWriterException("Trying to modify #US when it's read-only");
		}
		return AddToCache(s ?? string.Empty);
	}

	private uint AddToCache(string s)
	{
		cached.Add(s);
		uint num2 = (cachedDict[s] = nextOffset);
		nextOffset += (uint)GetRawDataSize(s);
		if (num2 > 16777215)
		{
			throw new ModuleWriterException("#US heap is too big");
		}
		return num2;
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
		foreach (string item in cached)
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
				WriteString(writer, item);
			}
			num += (uint)rawDataSize;
		}
	}

	private void WriteString(DataWriter writer, string s)
	{
		writer.WriteCompressedUInt32((uint)(s.Length * 2 + 1));
		byte value = 0;
		foreach (ushort num in s)
		{
			writer.WriteUInt16(num);
			if (num > 255 || (1 <= num && num <= 8) || (14 <= num && num <= 31) || num == 39 || num == 45 || num == 127)
			{
				value = 1;
			}
		}
		writer.WriteByte(value);
	}

	public int GetRawDataSize(string data)
	{
		return DataWriter.GetCompressedUInt32Length((uint)(data.Length * 2 + 1)) + data.Length * 2 + 1;
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
		foreach (string item in cached)
		{
			memStream.Position = 0L;
			memStream.SetLength(0L);
			WriteString(writer, item);
			yield return new KeyValuePair<uint, byte[]>(offset, memStream.ToArray());
			offset += (uint)(int)memStream.Length;
		}
	}
}
