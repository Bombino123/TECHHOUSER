using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Toolbelt.Drawing.Win32;

namespace Toolbelt.Drawing;

public class IconExtractor
{
	public static void Extract1stIconTo(string sourceFile, Stream stream)
	{
		IntPtr hModule = Kernel32.LoadLibraryEx(sourceFile, IntPtr.Zero, LOAD_LIBRARY.AS_DATAFILE);
		try
		{
			Kernel32.EnumResourceNames(hModule, RT.GROUP_ICON, delegate(IntPtr _hModule, RT type, IntPtr lpszName, IntPtr lParam)
			{
				ICONRESINF[] iconResInfo = GetIconResInfo(_hModule, lpszName);
				WriteIconData(hModule, iconResInfo, stream);
				return false;
			}, IntPtr.Zero);
		}
		finally
		{
			Kernel32.FreeLibrary(hModule);
		}
	}

	private static ICONRESINF[] GetIconResInfo(IntPtr hModule, IntPtr lpszName)
	{
		IntPtr hResInfo = Kernel32.FindResource(hModule, lpszName, RT.GROUP_ICON);
		IntPtr hResource = Kernel32.LoadResource(hModule, hResInfo);
		IntPtr ptrResource = Kernel32.LockResource(hResource);
		ICONRESHEAD iCONRESHEAD = (ICONRESHEAD)Marshal.PtrToStructure(ptrResource, typeof(ICONRESHEAD));
		int s1 = Marshal.SizeOf(typeof(ICONRESHEAD));
		int s2 = Marshal.SizeOf(typeof(ICONRESINF));
		return (from i in Enumerable.Range(0, iCONRESHEAD.Count)
			select (ICONRESINF)Marshal.PtrToStructure(ptrResource + s1 + s2 * i, typeof(ICONRESINF))).ToArray();
	}

	private static void WriteIconData(IntPtr hModule, ICONRESINF[] iconResInfos, Stream stream)
	{
		int num = Marshal.SizeOf(typeof(ICONFILEHEAD));
		int num2 = Marshal.SizeOf(typeof(ICONFILEINF));
		int address = num + num2 * iconResInfos.Length;
		var list = iconResInfos.Select(delegate(ICONRESINF iconResInf)
		{
			byte[] resourceBytes = GetResourceBytes(hModule, (IntPtr)iconResInf.ID, RT.ICON);
			ICONFILEINF iCONFILEINF = default(ICONFILEINF);
			iCONFILEINF.Cx = iconResInf.Cx;
			iCONFILEINF.Cy = iconResInf.Cy;
			iCONFILEINF.ColorCount = iconResInf.ColorCount;
			iCONFILEINF.Planes = iconResInf.Planes;
			iCONFILEINF.BitCount = iconResInf.BitCount;
			iCONFILEINF.Size = iconResInf.Size;
			iCONFILEINF.Address = (uint)address;
			ICONFILEINF iconFileInf = iCONFILEINF;
			address += resourceBytes.Length;
			return new
			{
				iconBytes = resourceBytes,
				iconFileInf = iconFileInf
			};
		}).ToList();
		byte[] array = StructureToBytes(new ICONFILEHEAD
		{
			Type = 1,
			Count = (ushort)iconResInfos.Length
		});
		stream.Write(array, 0, array.Length);
		list.ForEach(iconFile =>
		{
			byte[] array2 = StructureToBytes(iconFile.iconFileInf);
			stream.Write(array2, 0, array2.Length);
		});
		list.ForEach(iconFile =>
		{
			stream.Write(iconFile.iconBytes, 0, iconFile.iconBytes.Length);
		});
	}

	private static byte[] GetResourceBytes(IntPtr hModule, IntPtr lpszName, RT type)
	{
		IntPtr hResInfo = Kernel32.FindResource(hModule, lpszName, type);
		IntPtr source = Kernel32.LockResource(Kernel32.LoadResource(hModule, hResInfo));
		byte[] array = new byte[Kernel32.SizeofResource(hModule, hResInfo)];
		Marshal.Copy(source, array, 0, array.Length);
		return array;
	}

	private static byte[] StructureToBytes(object obj)
	{
		byte[] array = new byte[Marshal.SizeOf(obj)];
		GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
		Marshal.StructureToPtr(obj, gCHandle.AddrOfPinnedObject(), fDeleteOld: false);
		gCHandle.Free();
		return array;
	}
}
