using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
public struct VideoProcessorColorSpace
{
	[FieldOffset(0)]
	internal int _Usage;

	[FieldOffset(0)]
	internal int _RgbRange;

	[FieldOffset(0)]
	internal int _YCbCrMatrix;

	[FieldOffset(0)]
	internal int _YCbCrXvYCC;

	[FieldOffset(0)]
	internal int _NominalRange;

	[FieldOffset(0)]
	internal int _Reserved;

	public bool Usage
	{
		get
		{
			return (_Usage & 1) != 0;
		}
		set
		{
			_Usage = (_Usage & -2) | ((value ? 1 : 0) & 1);
		}
	}

	public bool RgbRange
	{
		get
		{
			return ((_RgbRange >> 1) & 1) != 0;
		}
		set
		{
			_RgbRange = (_RgbRange & -3) | (((value ? 1 : 0) & 1) << 1);
		}
	}

	public bool YCbCrMatrix
	{
		get
		{
			return ((_YCbCrMatrix >> 2) & 1) != 0;
		}
		set
		{
			_YCbCrMatrix = (_YCbCrMatrix & -5) | (((value ? 1 : 0) & 1) << 2);
		}
	}

	public bool YCbCrXvYCC
	{
		get
		{
			return ((_YCbCrXvYCC >> 3) & 1) != 0;
		}
		set
		{
			_YCbCrXvYCC = (_YCbCrXvYCC & -9) | (((value ? 1 : 0) & 1) << 3);
		}
	}

	public int NominalRange
	{
		get
		{
			return (_NominalRange >> 4) & 3;
		}
		set
		{
			_NominalRange = (_NominalRange & -49) | ((value & 3) << 4);
		}
	}

	public int Reserved
	{
		get
		{
			return (_Reserved >> 6) & 0x3FFFFFF;
		}
		set
		{
			_Reserved = (_Reserved & 0x3F) | ((value & 0x3FFFFFF) << 6);
		}
	}
}
