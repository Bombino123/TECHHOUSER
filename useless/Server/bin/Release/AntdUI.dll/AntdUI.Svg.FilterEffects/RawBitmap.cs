using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace AntdUI.Svg.FilterEffects;

internal sealed class RawBitmap : IDisposable
{
	private Bitmap _originBitmap;

	private BitmapData _bitmapData;

	private IntPtr _ptr;

	private int _bytes;

	private byte[] _argbValues;

	public int Stride => _bitmapData.Stride;

	public int Width => _bitmapData.Width;

	public int Height => _bitmapData.Height;

	public byte[] ArgbValues
	{
		get
		{
			return _argbValues;
		}
		set
		{
			_argbValues = value;
		}
	}

	public Bitmap Bitmap
	{
		get
		{
			Marshal.Copy(_argbValues, 0, _ptr, _bytes);
			return _originBitmap;
		}
	}

	public RawBitmap(Bitmap originBitmap)
	{
		_originBitmap = originBitmap;
		_bitmapData = _originBitmap.LockBits(new Rectangle(0, 0, ((Image)_originBitmap).Width, ((Image)_originBitmap).Height), (ImageLockMode)3, (PixelFormat)2498570);
		_ptr = _bitmapData.Scan0;
		_bytes = Stride * ((Image)_originBitmap).Height;
		_argbValues = new byte[_bytes];
		Marshal.Copy(_ptr, _argbValues, 0, _bytes);
	}

	public void Dispose()
	{
		_originBitmap.UnlockBits(_bitmapData);
	}
}
