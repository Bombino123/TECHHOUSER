namespace NAudio.Wave.Compression;

public class AcmFormatTag
{
	private AcmFormatTagDetails formatTagDetails;

	public int FormatTagIndex => formatTagDetails.formatTagIndex;

	public WaveFormatEncoding FormatTag => (WaveFormatEncoding)(ushort)formatTagDetails.formatTag;

	public int FormatSize => formatTagDetails.formatSize;

	public AcmDriverDetailsSupportFlags SupportFlags => formatTagDetails.supportFlags;

	public int StandardFormatsCount => formatTagDetails.standardFormatsCount;

	public string FormatDescription => formatTagDetails.formatDescription;

	internal AcmFormatTag(AcmFormatTagDetails formatTagDetails)
	{
		this.formatTagDetails = formatTagDetails;
	}
}
