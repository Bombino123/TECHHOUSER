using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Vestris.ResourceLib;

[ComVisible(true)]
public class FontResource : GenericResource
{
	public FontResource()
		: base(IntPtr.Zero, IntPtr.Zero, new ResourceId(Kernel32.ResourceTypes.RT_FONT), null, ResourceUtil.NEUTRALLANGID, 0)
	{
	}

	public FontResource(IntPtr hModule, IntPtr hResource, ResourceId type, ResourceId name, ushort language, int size)
		: base(hModule, hResource, type, name, language, size)
	{
	}

	internal override IntPtr Read(IntPtr hModule, IntPtr lpRes)
	{
		return base.Read(hModule, lpRes);
	}

	internal override void Write(BinaryWriter w)
	{
		base.Write(w);
	}
}
