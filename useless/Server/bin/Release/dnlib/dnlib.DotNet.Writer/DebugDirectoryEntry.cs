using dnlib.DotNet.Pdb;

namespace dnlib.DotNet.Writer;

public sealed class DebugDirectoryEntry
{
	public IMAGE_DEBUG_DIRECTORY DebugDirectory;

	public readonly IChunk Chunk;

	public DebugDirectoryEntry(IChunk chunk)
	{
		Chunk = chunk;
	}
}
