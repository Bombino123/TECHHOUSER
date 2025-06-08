using System.Drawing;

namespace GMap.NET.WindowsForms;

public class GMapImage : PureImage
{
	public Image Img;

	public override void Dispose()
	{
		if (Img != null)
		{
			Img.Dispose();
			Img = null;
		}
		if (base.Data != null)
		{
			base.Data.Dispose();
			base.Data = null;
		}
	}
}
