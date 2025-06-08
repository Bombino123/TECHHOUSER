using System.IO;

namespace NAudio.Wave;

public class Mp3FileReader : Mp3FileReaderBase
{
	public Mp3FileReader(string mp3FileName)
		: base((Stream)File.OpenRead(mp3FileName), new FrameDecompressorBuilder(CreateAcmFrameDecompressor), true)
	{
	}//IL_000e: Unknown result type (might be due to invalid IL or missing references)
	//IL_0019: Expected O, but got Unknown


	public Mp3FileReader(Stream inputStream)
		: base(inputStream, new FrameDecompressorBuilder(CreateAcmFrameDecompressor), false)
	{
	}//IL_0009: Unknown result type (might be due to invalid IL or missing references)
	//IL_0014: Expected O, but got Unknown


	public static IMp3FrameDecompressor CreateAcmFrameDecompressor(WaveFormat mp3Format)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		return (IMp3FrameDecompressor)new AcmMp3FrameDecompressor(mp3Format);
	}
}
