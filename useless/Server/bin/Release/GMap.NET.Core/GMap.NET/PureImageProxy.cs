using System.IO;

namespace GMap.NET;

public abstract class PureImageProxy
{
	public abstract PureImage FromStream(Stream stream);

	public abstract bool Save(Stream stream, PureImage image);

	public PureImage FromArray(byte[] data)
	{
		MemoryStream memoryStream = new MemoryStream(data, 0, data.Length, writable: false, publiclyVisible: true);
		PureImage pureImage = FromStream(memoryStream);
		if (pureImage != null)
		{
			memoryStream.Position = 0L;
			pureImage.Data = memoryStream;
		}
		return pureImage;
	}
}
