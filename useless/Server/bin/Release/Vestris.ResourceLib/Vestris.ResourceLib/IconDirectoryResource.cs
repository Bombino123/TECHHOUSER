using System;

namespace Vestris.ResourceLib;

public class IconDirectoryResource : DirectoryResource<IconResource>
{
	internal IconDirectoryResource(IntPtr hModule, IntPtr hResource, ResourceId type, ResourceId name, ushort language, int size)
		: base(hModule, hResource, type, name, language, size)
	{
	}

	public IconDirectoryResource()
		: base(Kernel32.ResourceTypes.RT_GROUP_ICON)
	{
	}

	public IconDirectoryResource(IconFile iconFile)
		: base(Kernel32.ResourceTypes.RT_GROUP_ICON)
	{
		for (int i = 0; i < iconFile.Icons.Count; i++)
		{
			IconResource item = new IconResource(iconFile.Icons[i], new ResourceId((uint)(i + 1)), _language);
			base.Icons.Add(item);
		}
	}
}
