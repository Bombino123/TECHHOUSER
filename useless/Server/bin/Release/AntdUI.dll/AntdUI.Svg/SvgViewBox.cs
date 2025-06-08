using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI.Svg;

public struct SvgViewBox
{
	public static readonly SvgViewBox Empty;

	public float MinX { get; set; }

	public float MinY { get; set; }

	public float Width { get; set; }

	public float Height { get; set; }

	public static implicit operator RectangleF(SvgViewBox value)
	{
		return new RectangleF(value.MinX, value.MinY, value.Width, value.Height);
	}

	public static implicit operator SvgViewBox(RectangleF value)
	{
		return new SvgViewBox(value.X, value.Y, value.Width, value.Height);
	}

	public SvgViewBox(float minX, float minY, float width, float height)
	{
		this = default(SvgViewBox);
		MinX = minX;
		MinY = minY;
		Width = width;
		Height = height;
	}

	public override bool Equals(object obj)
	{
		if (obj is SvgViewBox)
		{
			return Equals((SvgViewBox)obj);
		}
		return false;
	}

	public bool Equals(SvgViewBox other)
	{
		if (MinX == other.MinX && MinY == other.MinY && Width == other.Width)
		{
			return Height == other.Height;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return 0 + 1000000007 * MinX.GetHashCode() + 1000000009 * MinY.GetHashCode() + 1000000021 * Width.GetHashCode() + 1000000033 * Height.GetHashCode();
	}

	public static bool operator ==(SvgViewBox lhs, SvgViewBox rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(SvgViewBox lhs, SvgViewBox rhs)
	{
		return !(lhs == rhs);
	}

	public void AddViewBoxTransform(SvgAspectRatio aspectRatio, ISvgRenderer renderer, SvgFragment frag)
	{
		float dx = frag?.X.ToDeviceValue(renderer, UnitRenderingType.Horizontal, frag) ?? 0f;
		float dy = frag?.Y.ToDeviceValue(renderer, UnitRenderingType.Vertical, frag) ?? 0f;
		if (Equals(Empty))
		{
			renderer.TranslateTransform(dx, dy, (MatrixOrder)0);
			return;
		}
		float num = frag?.Width.ToDeviceValue(renderer, UnitRenderingType.Horizontal, frag) ?? Width;
		float num2 = frag?.Height.ToDeviceValue(renderer, UnitRenderingType.Vertical, frag) ?? Height;
		float num3 = num / Width;
		float num4 = num2 / Height;
		float num5 = (0f - MinX) * num3;
		float num6 = (0f - MinY) * num4;
		if (aspectRatio == null)
		{
			aspectRatio = new SvgAspectRatio(SvgPreserveAspectRatio.xMidYMid, slice: false);
		}
		if (aspectRatio.Align != SvgPreserveAspectRatio.none)
		{
			if (aspectRatio.Slice)
			{
				num3 = Math.Max(num3, num4);
				num4 = Math.Max(num3, num4);
			}
			else
			{
				num3 = Math.Min(num3, num4);
				num4 = Math.Min(num3, num4);
			}
			float num7 = Width / 2f * num3;
			float num8 = Height / 2f * num4;
			float num9 = num / 2f;
			float num10 = num2 / 2f;
			num5 = (0f - MinX) * num3;
			num6 = (0f - MinY) * num4;
			switch (aspectRatio.Align)
			{
			case SvgPreserveAspectRatio.xMidYMin:
				num5 += num9 - num7;
				break;
			case SvgPreserveAspectRatio.xMaxYMin:
				num5 += num - Width * num3;
				break;
			case SvgPreserveAspectRatio.xMinYMid:
				num6 += num10 - num8;
				break;
			case SvgPreserveAspectRatio.xMidYMid:
				num5 += num9 - num7;
				num6 += num10 - num8;
				break;
			case SvgPreserveAspectRatio.xMaxYMid:
				num5 += num - Width * num3;
				num6 += num10 - num8;
				break;
			case SvgPreserveAspectRatio.xMinYMax:
				num6 += num2 - Height * num4;
				break;
			case SvgPreserveAspectRatio.xMidYMax:
				num5 += num9 - num7;
				num6 += num2 - Height * num4;
				break;
			case SvgPreserveAspectRatio.xMaxYMax:
				num5 += num - Width * num3;
				num6 += num2 - Height * num4;
				break;
			}
		}
		renderer.TranslateTransform(dx, dy, (MatrixOrder)0);
		renderer.TranslateTransform(num5, num6, (MatrixOrder)0);
		renderer.ScaleTransform(num3, num4, (MatrixOrder)0);
	}
}
