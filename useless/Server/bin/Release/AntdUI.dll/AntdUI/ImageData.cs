using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace AntdUI;

public class ImageData : IDisposable
{
	private byte[,] _red;

	private byte[,] _green;

	private byte[,] _blue;

	private byte[,] _alpha;

	private bool _disposed;

	public byte[,] A
	{
		get
		{
			return _alpha;
		}
		set
		{
			_alpha = value;
		}
	}

	public byte[,] B
	{
		get
		{
			return _blue;
		}
		set
		{
			_blue = value;
		}
	}

	public byte[,] G
	{
		get
		{
			return _green;
		}
		set
		{
			_green = value;
		}
	}

	public byte[,] R
	{
		get
		{
			return _red;
		}
		set
		{
			_red = value;
		}
	}

	public ImageData Clone()
	{
		return new ImageData
		{
			A = (byte[,])_alpha.Clone(),
			B = (byte[,])_blue.Clone(),
			G = (byte[,])_green.Clone(),
			R = (byte[,])_red.Clone()
		};
	}

	public void FromBitmap(Bitmap srcBmp)
	{
		int width = ((Image)srcBmp).Width;
		int height = ((Image)srcBmp).Height;
		_alpha = new byte[width, height];
		_blue = new byte[width, height];
		_green = new byte[width, height];
		_red = new byte[width, height];
		BitmapData val = srcBmp.LockBits(new Rectangle(0, 0, width, height), (ImageLockMode)3, (PixelFormat)2498570);
		IntPtr scan = val.Scan0;
		int num = val.Stride * ((Image)srcBmp).Height;
		byte[] array = new byte[num];
		Marshal.Copy(scan, array, 0, num);
		int num2 = val.Stride - width * 4;
		int num3 = 0;
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				_blue[j, i] = array[num3];
				_green[j, i] = array[num3 + 1];
				_red[j, i] = array[num3 + 2];
				_alpha[j, i] = array[num3 + 3];
				num3 += 4;
			}
			num3 += num2;
		}
		srcBmp.UnlockBits(val);
	}

	public Bitmap ToBitmap()
	{
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		int num = 0;
		int num2 = 0;
		if (_alpha != null)
		{
			num = Math.Max(num, _alpha.GetLength(0));
			num2 = Math.Max(num2, _alpha.GetLength(1));
		}
		if (_blue != null)
		{
			num = Math.Max(num, _blue.GetLength(0));
			num2 = Math.Max(num2, _blue.GetLength(1));
		}
		if (_green != null)
		{
			num = Math.Max(num, _green.GetLength(0));
			num2 = Math.Max(num2, _green.GetLength(1));
		}
		if (_red != null)
		{
			num = Math.Max(num, _red.GetLength(0));
			num2 = Math.Max(num2, _red.GetLength(1));
		}
		Bitmap val = new Bitmap(num, num2, (PixelFormat)2498570);
		BitmapData val2 = val.LockBits(new Rectangle(0, 0, num, num2), (ImageLockMode)3, (PixelFormat)2498570);
		int num3 = val2.Stride * ((Image)val).Height;
		byte[] array = new byte[num3];
		int num4 = val2.Stride - num * 4;
		int num5 = 0;
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				array[num5] = (byte)(checkArray(_blue, j, i) ? _blue[j, i] : 0);
				array[num5 + 1] = (byte)(checkArray(_green, j, i) ? _green[j, i] : 0);
				array[num5 + 2] = (byte)(checkArray(_red, j, i) ? _red[j, i] : 0);
				array[num5 + 3] = (checkArray(_alpha, j, i) ? _alpha[j, i] : byte.MaxValue);
				num5 += 4;
			}
			num5 += num4;
		}
		Marshal.Copy(array, 0, val2.Scan0, num3);
		val.UnlockBits(val2);
		return val;
	}

	private static bool checkArray(byte[,] array, int x, int y)
	{
		if (array == null)
		{
			return false;
		}
		if (x < array.GetLength(0) && y < array.GetLength(1))
		{
			return true;
		}
		return false;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				_alpha = null;
				_blue = null;
				_green = null;
				_red = null;
			}
			_disposed = true;
		}
	}
}
