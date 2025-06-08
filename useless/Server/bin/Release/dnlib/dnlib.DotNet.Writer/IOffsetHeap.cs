using System.Collections.Generic;

namespace dnlib.DotNet.Writer;

public interface IOffsetHeap<TValue>
{
	int GetRawDataSize(TValue data);

	void SetRawData(uint offset, byte[] rawData);

	IEnumerable<KeyValuePair<uint, byte[]>> GetAllRawData();
}
