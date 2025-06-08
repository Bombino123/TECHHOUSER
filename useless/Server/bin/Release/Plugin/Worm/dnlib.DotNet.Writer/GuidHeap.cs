using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public sealed class GuidHeap : HeapBase, IOffsetHeap<Guid>
{
	private readonly Dictionary<Guid, uint> guids = new Dictionary<Guid, uint>();

	private Dictionary<uint, byte[]> userRawData;

	public override string Name => "#GUID";

	public uint Add(Guid? guid)
	{
		if (isReadOnly)
		{
			throw new ModuleWriterException("Trying to modify #GUID when it's read-only");
		}
		if (!guid.HasValue)
		{
			return 0u;
		}
		if (guids.TryGetValue(guid.Value, out var value))
		{
			return value;
		}
		value = (uint)(guids.Count + 1);
		guids.Add(guid.Value, value);
		return value;
	}

	public override uint GetRawLength()
	{
		return (uint)(guids.Count * 16);
	}

	protected override void WriteToImpl(DataWriter writer)
	{
		uint num = 0u;
		foreach (KeyValuePair<Guid, uint> guid in guids)
		{
			if (userRawData == null || !userRawData.TryGetValue(num, out var value))
			{
				value = guid.Key.ToByteArray();
			}
			writer.WriteBytes(value);
			num += 16;
		}
	}

	public int GetRawDataSize(Guid data)
	{
		return 16;
	}

	public void SetRawData(uint offset, byte[] rawData)
	{
		if (rawData == null || rawData.Length != 16)
		{
			throw new ArgumentException("Invalid size of GUID raw data");
		}
		if (userRawData == null)
		{
			userRawData = new Dictionary<uint, byte[]>();
		}
		userRawData[offset] = rawData;
	}

	public IEnumerable<KeyValuePair<uint, byte[]>> GetAllRawData()
	{
		uint offset = 0u;
		foreach (KeyValuePair<Guid, uint> guid in guids)
		{
			yield return new KeyValuePair<uint, byte[]>(offset, guid.Key.ToByteArray());
			offset += 16;
		}
	}
}
