using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using dnlib.DotNet.MD;
using dnlib.IO;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public sealed class StringsHeap : HeapBase, IOffsetHeap<UTF8String>
{
	private sealed class StringsOffsetInfo
	{
		public readonly UTF8String Value;

		public readonly uint StringsId;

		public uint StringsOffset;

		public StringsOffsetInfo(UTF8String value, uint stringsId)
		{
			Value = value;
			StringsId = stringsId;
		}

		public override string ToString()
		{
			return $"{StringsId:X8} {StringsOffset:X4} {Value.String}";
		}
	}

	private readonly Dictionary<UTF8String, uint> cachedDict = new Dictionary<UTF8String, uint>(UTF8StringEqualityComparer.Instance);

	private readonly List<UTF8String> cached = new List<UTF8String>();

	private uint nextOffset = 1u;

	private byte[] originalData;

	private Dictionary<uint, byte[]> userRawData;

	private readonly Dictionary<UTF8String, StringsOffsetInfo> toStringsOffsetInfo = new Dictionary<UTF8String, StringsOffsetInfo>(UTF8StringEqualityComparer.Instance);

	private readonly Dictionary<uint, StringsOffsetInfo> offsetIdToInfo = new Dictionary<uint, StringsOffsetInfo>();

	private readonly List<StringsOffsetInfo> stringsOffsetInfos = new List<StringsOffsetInfo>();

	private const uint STRINGS_ID_FLAG = 2147483648u;

	private uint stringsId = 2147483648u;

	private static readonly Comparison<StringsOffsetInfo> Comparison_StringsOffsetInfoSorter = StringsOffsetInfoSorter;

	public override string Name => "#Strings";

	public void Populate(StringsStream stringsStream)
	{
		if (isReadOnly)
		{
			throw new ModuleWriterException("Trying to modify #Strings when it's read-only");
		}
		if (originalData != null)
		{
			throw new InvalidOperationException("Can't call method twice");
		}
		if (nextOffset != 1)
		{
			throw new InvalidOperationException("Add() has already been called");
		}
		if (stringsStream != null && stringsStream.StreamLength != 0)
		{
			DataReader reader = stringsStream.CreateReader();
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
			byte[] array = reader.TryReadBytesUntil(0);
			if (array == null)
			{
				break;
			}
			reader.ReadByte();
			if (array.Length != 0)
			{
				UTF8String key = new UTF8String(array);
				if (!cachedDict.ContainsKey(key))
				{
					cachedDict[key] = position;
				}
			}
		}
	}

	internal void AddOptimizedStringsAndSetReadOnly()
	{
		if (isReadOnly)
		{
			throw new ModuleWriterException("Trying to modify #Strings when it's read-only");
		}
		SetReadOnly();
		stringsOffsetInfos.Sort(Comparison_StringsOffsetInfoSorter);
		StringsOffsetInfo stringsOffsetInfo = null;
		foreach (StringsOffsetInfo stringsOffsetInfo2 in stringsOffsetInfos)
		{
			if (stringsOffsetInfo != null && EndsWith(stringsOffsetInfo.Value, stringsOffsetInfo2.Value))
			{
				stringsOffsetInfo2.StringsOffset = stringsOffsetInfo.StringsOffset + (uint)(stringsOffsetInfo.Value.Data.Length - stringsOffsetInfo2.Value.Data.Length);
			}
			else
			{
				stringsOffsetInfo2.StringsOffset = AddToCache(stringsOffsetInfo2.Value);
			}
			stringsOffsetInfo = stringsOffsetInfo2;
		}
	}

	private static bool EndsWith(UTF8String s, UTF8String value)
	{
		byte[] data = s.Data;
		byte[] data2 = value.Data;
		int num = data.Length - data2.Length;
		if (num < 0)
		{
			return false;
		}
		for (int i = 0; i < data2.Length; i++)
		{
			if (data[num] != data2[i])
			{
				return false;
			}
			num++;
		}
		return true;
	}

	private static int StringsOffsetInfoSorter(StringsOffsetInfo a, StringsOffsetInfo b)
	{
		byte[] data = a.Value.Data;
		byte[] data2 = b.Value.Data;
		int num = data.Length - 1;
		int num2 = data2.Length - 1;
		for (int num3 = Math.Min(data.Length, data2.Length); num3 > 0; num3--)
		{
			int num4 = data[num] - data2[num2];
			if (num4 != 0)
			{
				return num4;
			}
			num--;
			num2--;
		}
		return data2.Length - data.Length;
	}

	public uint Add(UTF8String s)
	{
		if (isReadOnly)
		{
			throw new ModuleWriterException("Trying to modify #Strings when it's read-only");
		}
		if (UTF8String.IsNullOrEmpty(s))
		{
			return 0u;
		}
		if (toStringsOffsetInfo.TryGetValue(s, out var value))
		{
			return value.StringsId;
		}
		if (cachedDict.TryGetValue(s, out var value2))
		{
			return value2;
		}
		if (Array.IndexOf(s.Data, (byte)0) >= 0)
		{
			throw new ArgumentException("Strings in the #Strings heap can't contain NUL bytes");
		}
		value = new StringsOffsetInfo(s, stringsId++);
		toStringsOffsetInfo[s] = value;
		offsetIdToInfo[value.StringsId] = value;
		stringsOffsetInfos.Add(value);
		return value.StringsId;
	}

	public uint GetOffset(uint offsetId)
	{
		if (!isReadOnly)
		{
			throw new ModuleWriterException("This method can only be called after all strings have been added and this heap is read-only");
		}
		if ((offsetId & 0x80000000u) == 0)
		{
			return offsetId;
		}
		if (offsetIdToInfo.TryGetValue(offsetId, out var value))
		{
			return value.StringsOffset;
		}
		throw new ArgumentOutOfRangeException("offsetId");
	}

	public uint Create(UTF8String s)
	{
		if (isReadOnly)
		{
			throw new ModuleWriterException("Trying to modify #Strings when it's read-only");
		}
		if (UTF8String.IsNullOrEmpty(s))
		{
			s = UTF8String.Empty;
		}
		if (Array.IndexOf(s.Data, (byte)0) >= 0)
		{
			throw new ArgumentException("Strings in the #Strings heap can't contain NUL bytes");
		}
		return AddToCache(s);
	}

	private uint AddToCache(UTF8String s)
	{
		cached.Add(s);
		uint result = (cachedDict[s] = nextOffset);
		nextOffset += (uint)(s.Data.Length + 1);
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
		foreach (UTF8String item in cached)
		{
			if (userRawData != null && userRawData.TryGetValue(num, out var value))
			{
				if (value.Length != item.Data.Length + 1)
				{
					throw new InvalidOperationException("Invalid length of raw data");
				}
				writer.WriteBytes(value);
			}
			else
			{
				writer.WriteBytes(item.Data);
				writer.WriteByte(0);
			}
			num += (uint)(item.Data.Length + 1);
		}
	}

	public int GetRawDataSize(UTF8String data)
	{
		return data.Data.Length + 1;
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
		uint offset = ((originalData == null) ? 1u : ((uint)originalData.Length));
		foreach (UTF8String item in cached)
		{
			byte[] rawData = new byte[item.Data.Length + 1];
			Array.Copy(item.Data, rawData, item.Data.Length);
			yield return new KeyValuePair<uint, byte[]>(offset, rawData);
			offset += (uint)rawData.Length;
		}
	}
}
