using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public interface IOffsetHeap<TValue>
{
	int GetRawDataSize(TValue data);

	void SetRawData(uint offset, byte[] rawData);

	IEnumerable<KeyValuePair<uint, byte[]>> GetAllRawData();
}
