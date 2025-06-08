using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using GMap.NET.Internals;
using GMap.NET.MapProviders;

namespace GMap.NET.WindowsForms;

public class GMapImageProxy : PureImageProxy
{
	public static readonly GMapImageProxy Instance = new GMapImageProxy();

	internal ColorMatrix ColorMatrix;

	private static readonly bool Win7OrLater = Stuff.IsRunningOnWin7OrLater();

	private GMapImageProxy()
	{
	}

	public static void Enable()
	{
		GMapProvider.TileImageProxy = (PureImageProxy)(object)Instance;
	}

	public override PureImage FromStream(Stream stream)
	{
		try
		{
			Image val = Image.FromStream(stream, true, !Win7OrLater);
			if (val != null)
			{
				return (PureImage)(object)new GMapImage
				{
					Img = (Image)((ColorMatrix != null) ? ((object)ApplyColorMatrix(val, ColorMatrix)) : ((object)val))
				};
			}
		}
		catch (Exception)
		{
		}
		return null;
	}

	public override bool Save(Stream stream, PureImage image)
	{
		GMapImage gMapImage = image as GMapImage;
		bool result = true;
		if (gMapImage.Img != null)
		{
			try
			{
				gMapImage.Img.Save(stream, ImageFormat.Png);
			}
			catch
			{
				try
				{
					stream.Seek(0L, SeekOrigin.Begin);
					gMapImage.Img.Save(stream, ImageFormat.Jpeg);
				}
				catch
				{
					result = false;
				}
			}
		}
		else
		{
			result = false;
		}
		return result;
	}

	private Bitmap ApplyColorMatrix(Image original, ColorMatrix matrix)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		Bitmap val = new Bitmap(original.Width, original.Height);
		try
		{
			Graphics val2 = Graphics.FromImage((Image)(object)val);
			try
			{
				ImageAttributes val3 = new ImageAttributes();
				try
				{
					val3.SetColorMatrix(matrix);
					val2.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height), 0, 0, original.Width, original.Height, (GraphicsUnit)2, val3);
					return val;
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)original)?.Dispose();
		}
	}
}
