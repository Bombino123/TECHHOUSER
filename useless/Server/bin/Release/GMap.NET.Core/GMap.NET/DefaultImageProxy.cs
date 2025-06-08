using System.IO;

namespace GMap.NET;

public class DefaultImageProxy : PureImageProxy
{
	private class DefaultImage : PureImage
	{
		public override void Dispose()
		{
		}
	}

	public static readonly DefaultImageProxy Instance = new DefaultImageProxy();

	private DefaultImageProxy()
	{
	}

	public override PureImage FromStream(Stream stream)
	{
		return new DefaultImage();
	}

	public override bool Save(Stream stream, PureImage image)
	{
		if (image.Data != null)
		{
			image.Data.WriteTo(stream);
			image.Data.Position = 0L;
			return true;
		}
		return false;
	}
}
