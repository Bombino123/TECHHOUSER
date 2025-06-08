namespace dnlib.DotNet.Writer;

public interface IHeap : IChunk
{
	string Name { get; }

	bool IsEmpty { get; }

	void SetReadOnly();
}
