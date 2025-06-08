using System;
using System.Runtime.InteropServices;

namespace Vestris.ResourceLib;

[ComVisible(true)]
public class CursorResource : IconImageResource
{
	private ushort _hotspotx;

	private ushort _hotspoty;

	public ushort HotspotX
	{
		get
		{
			return _hotspotx;
		}
		set
		{
			_hotspotx = value;
		}
	}

	public ushort HotspotY
	{
		get
		{
			return _hotspoty;
		}
		set
		{
			_hotspoty = value;
		}
	}

	internal CursorResource(IntPtr hModule, IntPtr hResource, ResourceId type, ResourceId name, ushort language, int size)
		: base(hModule, hResource, type, name, language, size)
	{
	}

	public CursorResource()
		: base(new ResourceId(Kernel32.ResourceTypes.RT_CURSOR))
	{
	}

	public CursorResource(IconFileIcon icon, ResourceId id, ushort language)
		: base(icon, new ResourceId(Kernel32.ResourceTypes.RT_CURSOR), id, language)
	{
	}

	public override void SaveIconTo(string filename)
	{
		byte[] array = new byte[base.Image.Data.Length + 4];
		Buffer.BlockCopy(base.Image.Data, 0, array, 4, base.Image.Data.Length);
		array[0] = (byte)(HotspotX & 0xFFu);
		array[1] = (byte)(HotspotX >> 8);
		array[2] = (byte)(HotspotY & 0xFFu);
		array[3] = (byte)(HotspotY >> 8);
		Resource.SaveTo(filename, _type, new ResourceId(_header.nID), _language, array);
	}

	internal override void ReadImage(IntPtr dibBits, uint size)
	{
		_hotspotx = (ushort)Marshal.ReadInt16(dibBits);
		dibBits = new IntPtr(dibBits.ToInt64() + 2);
		_hotspoty = (ushort)Marshal.ReadInt16(dibBits);
		dibBits = new IntPtr(dibBits.ToInt64() + 2);
		base.ReadImage(dibBits, size - 4);
	}
}
