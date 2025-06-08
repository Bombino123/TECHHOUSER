using System.Drawing;
using System.Runtime.InteropServices;

namespace AntdUI;

[StructLayout(LayoutKind.Explicit)]
public struct ColorBgra
{
	[FieldOffset(0)]
	public uint Bgra;

	[FieldOffset(0)]
	public byte Blue;

	[FieldOffset(1)]
	public byte Green;

	[FieldOffset(2)]
	public byte Red;

	[FieldOffset(3)]
	public byte Alpha;

	public const byte SizeOf = 4;

	public ColorBgra(uint bgra)
	{
		this = default(ColorBgra);
		Bgra = bgra;
	}

	public ColorBgra(byte b, byte g, byte r, byte a = byte.MaxValue)
	{
		this = default(ColorBgra);
		Blue = b;
		Green = g;
		Red = r;
		Alpha = a;
	}

	public ColorBgra(Color color)
		: this(color.B, color.G, color.R, color.A)
	{
	}

	public static bool operator ==(ColorBgra c1, ColorBgra c2)
	{
		return c1.Bgra == c2.Bgra;
	}

	public static bool operator !=(ColorBgra c1, ColorBgra c2)
	{
		return c1.Bgra != c2.Bgra;
	}

	public override bool Equals(object? obj)
	{
		if (obj is ColorBgra colorBgra)
		{
			return colorBgra.Bgra == Bgra;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)Bgra;
	}

	public static implicit operator ColorBgra(uint color)
	{
		return new ColorBgra(color);
	}

	public static implicit operator uint(ColorBgra color)
	{
		return color.Bgra;
	}

	public Color ToColor()
	{
		return Color.FromArgb(Alpha, Red, Green, Blue);
	}

	public override string ToString()
	{
		return $"B: {Blue}, G: {Green}, R: {Red}, A: {Alpha}";
	}

	public static uint BgraToUInt32(uint b, uint g, uint r, uint a)
	{
		return b + (g << 8) + (r << 16) + (a << 24);
	}

	public static uint BgraToUInt32(byte b, byte g, byte r, byte a)
	{
		return (uint)(b + (g << 8) + (r << 16) + (a << 24));
	}
}
