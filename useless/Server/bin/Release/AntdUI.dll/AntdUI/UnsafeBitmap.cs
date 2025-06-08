using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace AntdUI;

public class UnsafeBitmap : IDisposable
{
	private Bitmap bitmap;

	private BitmapData bitmapData;

	public unsafe ColorBgra* Pointer { get; private set; }

	public bool IsLocked { get; private set; }

	public int Width { get; private set; }

	public int Height { get; private set; }

	public int PixelCount => Width * Height;

	public UnsafeBitmap(Bitmap bitmap, bool lockBitmap = false, ImageLockMode imageLockMode = 3)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		this.bitmap = bitmap;
		Width = ((Image)bitmap).Width;
		Height = ((Image)bitmap).Height;
		if (lockBitmap)
		{
			Lock(imageLockMode);
		}
	}

	public unsafe void Lock(ImageLockMode imageLockMode = 3)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (!IsLocked)
		{
			IsLocked = true;
			bitmapData = bitmap.LockBits(new Rectangle(0, 0, Width, Height), imageLockMode, (PixelFormat)2498570);
			Pointer = (ColorBgra*)bitmapData.Scan0.ToPointer();
		}
	}

	public unsafe void Unlock()
	{
		if (IsLocked)
		{
			bitmap.UnlockBits(bitmapData);
			bitmapData = null;
			Pointer = null;
			IsLocked = false;
		}
	}

	public static bool operator ==(UnsafeBitmap bmp1, UnsafeBitmap bmp2)
	{
		if ((object)bmp1 != bmp2)
		{
			return bmp1.Equals(bmp2);
		}
		return true;
	}

	public static bool operator !=(UnsafeBitmap bmp1, UnsafeBitmap bmp2)
	{
		return !(bmp1 == bmp2);
	}

	public override bool Equals(object? obj)
	{
		if (obj is UnsafeBitmap bmp)
		{
			return Compare(bmp, this);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return PixelCount;
	}

	public unsafe static bool Compare(UnsafeBitmap bmp1, UnsafeBitmap bmp2)
	{
		int pixelCount = bmp1.PixelCount;
		if (pixelCount != bmp2.PixelCount)
		{
			return false;
		}
		bmp1.Lock((ImageLockMode)1);
		bmp2.Lock((ImageLockMode)1);
		ColorBgra* ptr = bmp1.Pointer;
		ColorBgra* ptr2 = bmp2.Pointer;
		for (int i = 0; i < pixelCount; i++)
		{
			if (ptr->Bgra != ptr2->Bgra)
			{
				return false;
			}
			ptr++;
			ptr2++;
		}
		return true;
	}

	public unsafe bool IsTransparent()
	{
		int pixelCount = PixelCount;
		ColorBgra* ptr = Pointer;
		for (int i = 0; i < pixelCount; i++)
		{
			if (ptr->Alpha < byte.MaxValue)
			{
				return true;
			}
			ptr++;
		}
		return false;
	}

	public unsafe ColorBgra GetPixel(int i)
	{
		return Pointer[i];
	}

	public unsafe ColorBgra GetPixel(int x, int y)
	{
		return Pointer[x + y * Width];
	}

	public unsafe void SetPixel(int i, ColorBgra color)
	{
		Pointer[i] = color;
	}

	public unsafe void SetPixel(int i, uint color)
	{
		Pointer[i] = color;
	}

	public unsafe void SetPixel(int x, int y, ColorBgra color)
	{
		Pointer[x + y * Width] = color;
	}

	public unsafe void SetPixel(int x, int y, uint color)
	{
		Pointer[x + y * Width] = color;
	}

	public unsafe void ClearPixel(int i)
	{
		Pointer[i] = 0u;
	}

	public unsafe void ClearPixel(int x, int y)
	{
		Pointer[x + y * Width] = 0u;
	}

	public void Dispose()
	{
		Unlock();
	}
}
