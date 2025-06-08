using System.IO;

namespace NAudio.SoundFont;

internal class SampleDataChunk
{
	public byte[] SampleData { get; private set; }

	public SampleDataChunk(RiffChunk chunk)
	{
		string text = chunk.ReadChunkID();
		if (text != "sdta")
		{
			throw new InvalidDataException("Not a sample data chunk (" + text + ")");
		}
		SampleData = chunk.GetData();
	}
}
