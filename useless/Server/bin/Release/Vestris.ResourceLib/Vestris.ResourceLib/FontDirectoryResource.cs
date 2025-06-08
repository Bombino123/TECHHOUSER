using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Vestris.ResourceLib;

public class FontDirectoryResource : Resource
{
	private List<FontDirectoryEntry> _fonts = new List<FontDirectoryEntry>();

	private byte[] _reserved;

	public List<FontDirectoryEntry> Fonts
	{
		get
		{
			return _fonts;
		}
		set
		{
			_fonts = value;
		}
	}

	public FontDirectoryResource()
		: base(IntPtr.Zero, IntPtr.Zero, new ResourceId(Kernel32.ResourceTypes.RT_FONTDIR), null, ResourceUtil.NEUTRALLANGID, 0)
	{
	}

	public FontDirectoryResource(IntPtr hModule, IntPtr hResource, ResourceId type, ResourceId name, ushort language, int size)
		: base(hModule, hResource, type, name, language, size)
	{
	}

	internal override IntPtr Read(IntPtr hModule, IntPtr lpRes)
	{
		IntPtr intPtr = lpRes;
		ushort num = (ushort)Marshal.ReadInt16(lpRes);
		lpRes = new IntPtr(lpRes.ToInt64() + 2);
		for (int i = 0; i < num; i++)
		{
			FontDirectoryEntry fontDirectoryEntry = new FontDirectoryEntry();
			lpRes = fontDirectoryEntry.Read(lpRes);
			_fonts.Add(fontDirectoryEntry);
		}
		int num2 = _size - (int)(lpRes.ToInt64() - intPtr.ToInt64());
		_reserved = new byte[num2];
		Marshal.Copy(lpRes, _reserved, 0, num2);
		return lpRes;
	}

	internal override void Write(BinaryWriter w)
	{
		w.Write((ushort)_fonts.Count);
		foreach (FontDirectoryEntry font in _fonts)
		{
			font.Write(w);
		}
		w.Write(_reserved);
	}
}
