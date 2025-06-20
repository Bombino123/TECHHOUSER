using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Vestris.ResourceLib;

[ComVisible(true)]
public class DirectoryResource<ImageResourceType> : Resource where ImageResourceType : IconImageResource, new()
{
	private Kernel32.GRPICONDIR _header;

	private List<ImageResourceType> _icons = new List<ImageResourceType>();

	public Kernel32.ResourceTypes ResourceType => _header.wType switch
	{
		1 => Kernel32.ResourceTypes.RT_ICON, 
		2 => Kernel32.ResourceTypes.RT_CURSOR, 
		_ => throw new NotSupportedException(), 
	};

	public List<ImageResourceType> Icons
	{
		get
		{
			return _icons;
		}
		set
		{
			_icons = value;
		}
	}

	internal DirectoryResource(IntPtr hModule, IntPtr hResource, ResourceId type, ResourceId name, ushort language, int size)
		: base(hModule, hResource, type, name, language, size)
	{
	}

	public DirectoryResource(Kernel32.ResourceTypes resourceType)
		: base(IntPtr.Zero, IntPtr.Zero, new ResourceId(resourceType), new ResourceId(1u), ResourceUtil.NEUTRALLANGID, Marshal.SizeOf(typeof(Kernel32.GRPICONDIR)))
	{
		switch (resourceType)
		{
		case Kernel32.ResourceTypes.RT_GROUP_CURSOR:
			_header.wType = 2;
			break;
		case Kernel32.ResourceTypes.RT_GROUP_ICON:
			_header.wType = 1;
			break;
		default:
			throw new NotSupportedException();
		}
	}

	public override void SaveTo(string filename)
	{
		base.SaveTo(filename);
		foreach (ImageResourceType icon in _icons)
		{
			icon.SaveIconTo(filename);
		}
	}

	internal override IntPtr Read(IntPtr hModule, IntPtr lpRes)
	{
		_icons.Clear();
		_header = (Kernel32.GRPICONDIR)Marshal.PtrToStructure(lpRes, typeof(Kernel32.GRPICONDIR));
		IntPtr intPtr = new IntPtr(lpRes.ToInt64() + Marshal.SizeOf((object)_header));
		for (ushort num = 0; num < _header.wImageCount; num++)
		{
			ImageResourceType val = new ImageResourceType();
			intPtr = val.Read(hModule, intPtr);
			_icons.Add(val);
		}
		return intPtr;
	}

	internal override void Write(BinaryWriter w)
	{
		w.Write(_header.wReserved);
		w.Write(_header.wType);
		w.Write((ushort)_icons.Count);
		ResourceUtil.PadToWORD(w);
		foreach (ImageResourceType icon in _icons)
		{
			icon.Write(w);
		}
	}
}
