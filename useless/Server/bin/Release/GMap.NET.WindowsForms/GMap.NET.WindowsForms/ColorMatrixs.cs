using System.Drawing.Imaging;

namespace GMap.NET.WindowsForms;

public static class ColorMatrixs
{
	public static readonly ColorMatrix GrayScale = new ColorMatrix(new float[5][]
	{
		new float[5] { 0.3f, 0.3f, 0.3f, 0f, 0f },
		new float[5] { 0.59f, 0.59f, 0.59f, 0f, 0f },
		new float[5] { 0.11f, 0.11f, 0.11f, 0f, 0f },
		new float[5] { 0f, 0f, 0f, 1f, 0f },
		new float[5] { 0f, 0f, 0f, 0f, 1f }
	});

	public static readonly ColorMatrix Negative = new ColorMatrix(new float[5][]
	{
		new float[5] { -1f, 0f, 0f, 0f, 0f },
		new float[5] { 0f, -1f, 0f, 0f, 0f },
		new float[5] { 0f, 0f, -1f, 0f, 0f },
		new float[5] { 0f, 0f, 0f, 1f, 0f },
		new float[5] { 1f, 1f, 1f, 0f, 1f }
	});
}
