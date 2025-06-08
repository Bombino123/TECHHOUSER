using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Vestris.ResourceLib;

public class BitmapResource : Resource
{
	private DeviceIndependentBitmap _bitmap;

	public DeviceIndependentBitmap Bitmap
	{
		get
		{
			return _bitmap;
		}
		set
		{
			_bitmap = value;
		}
	}

	public BitmapResource(IntPtr hModule, IntPtr hResource, ResourceId type, ResourceId name, ushort language, int size)
		: base(hModule, hResource, type, name, language, size)
	{
	}

	public BitmapResource()
		: base(IntPtr.Zero, IntPtr.Zero, new ResourceId(Kernel32.ResourceTypes.RT_BITMAP), new ResourceId(1u), 0, 0)
	{
	}

	internal override IntPtr Read(IntPtr hModule, IntPtr lpRes)
	{
		byte[] array = new byte[_size];
		Marshal.Copy(lpRes, array, 0, array.Length);
		_bitmap = new DeviceIndependentBitmap(array);
		return new IntPtr(lpRes.ToInt64() + _size);
	}

	internal override void Write(BinaryWriter w)
	{
		w.Write(_bitmap.Data);
	}
}
