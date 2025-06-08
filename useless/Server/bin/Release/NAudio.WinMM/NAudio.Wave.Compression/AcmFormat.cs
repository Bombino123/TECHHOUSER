namespace NAudio.Wave.Compression;

public class AcmFormat
{
	private readonly AcmFormatDetails formatDetails;

	public int FormatIndex => formatDetails.formatIndex;

	public WaveFormatEncoding FormatTag => (WaveFormatEncoding)(ushort)formatDetails.formatTag;

	public AcmDriverDetailsSupportFlags SupportFlags => formatDetails.supportFlags;

	public WaveFormat WaveFormat { get; private set; }

	public int WaveFormatByteSize => formatDetails.waveFormatByteSize;

	public string FormatDescription => formatDetails.formatDescription;

	internal AcmFormat(AcmFormatDetails formatDetails)
	{
		this.formatDetails = formatDetails;
		WaveFormat = WaveFormat.MarshalFromPtr(formatDetails.waveFormatPointer);
	}
}
