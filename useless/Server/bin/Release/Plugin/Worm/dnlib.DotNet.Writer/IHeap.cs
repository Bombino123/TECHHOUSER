using System.Runtime.InteropServices;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public interface IHeap : IChunk
{
	string Name { get; }

	bool IsEmpty { get; }

	void SetReadOnly();
}
