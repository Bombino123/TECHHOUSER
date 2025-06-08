using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Vestris.ResourceLib;

public class BitmapFile
{
	private Gdi32.BITMAPFILEHEADER _header;

	private DeviceIndependentBitmap _bitmap;

	public DeviceIndependentBitmap Bitmap => _bitmap;

	public BitmapFile(string filename)
	{
		byte[] array = File.ReadAllBytes(filename);
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf((object)_header));
		try
		{
			Marshal.Copy(array, 0, intPtr, Marshal.SizeOf((object)_header));
			_header = (Gdi32.BITMAPFILEHEADER)Marshal.PtrToStructure(intPtr, typeof(Gdi32.BITMAPFILEHEADER));
		}
		finally
		{
			Marshal.FreeHGlobal(intPtr);
		}
		int num = array.Length - Marshal.SizeOf((object)_header);
		byte[] array2 = new byte[num];
		Buffer.BlockCopy(array, Marshal.SizeOf((object)_header), array2, 0, num);
		_bitmap = new DeviceIndependentBitmap(array2);
	}
}
