using System;
using System.Runtime.InteropServices;

namespace Vestris.ResourceLib;

[ComVisible(true)]
public class CursorDirectoryResource : DirectoryResource<CursorResource>
{
	internal CursorDirectoryResource(IntPtr hModule, IntPtr hResource, ResourceId type, ResourceId name, ushort language, int size)
		: base(hModule, hResource, type, name, language, size)
	{
	}

	public CursorDirectoryResource()
		: base(Kernel32.ResourceTypes.RT_GROUP_CURSOR)
	{
	}

	public CursorDirectoryResource(IconFile iconFile)
		: base(Kernel32.ResourceTypes.RT_GROUP_CURSOR)
	{
		for (ushort num = 0; num < iconFile.Icons.Count; num++)
		{
			CursorResource item = new CursorResource(iconFile.Icons[num], new ResourceId(num), _language)
			{
				HotspotX = iconFile.Icons[num].Header.wPlanes,
				HotspotY = iconFile.Icons[num].Header.wBitsPerPixel
			};
			base.Icons.Add(item);
		}
	}
}
