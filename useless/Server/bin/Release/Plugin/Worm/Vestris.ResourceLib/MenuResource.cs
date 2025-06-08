using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Vestris.ResourceLib;

[ComVisible(true)]
public class MenuResource : Resource
{
	private MenuTemplateBase _menu;

	public MenuTemplateBase Menu
	{
		get
		{
			return _menu;
		}
		set
		{
			_menu = value;
		}
	}

	public MenuResource()
		: base(IntPtr.Zero, IntPtr.Zero, new ResourceId(Kernel32.ResourceTypes.RT_MENU), null, ResourceUtil.NEUTRALLANGID, 0)
	{
	}

	public MenuResource(IntPtr hModule, IntPtr hResource, ResourceId type, ResourceId name, ushort language, int size)
		: base(hModule, hResource, type, name, language, size)
	{
	}

	internal override IntPtr Read(IntPtr hModule, IntPtr lpRes)
	{
		ushort num = (ushort)Marshal.ReadInt16(lpRes);
		switch (num)
		{
		case 0:
			_menu = new MenuTemplate();
			break;
		case 1:
			_menu = new MenuExTemplate();
			break;
		default:
			throw new NotSupportedException($"Unexpected menu header version {num}");
		}
		return _menu.Read(lpRes);
	}

	internal override void Write(BinaryWriter w)
	{
		_menu.Write(w);
	}

	public override string ToString()
	{
		return $"{base.Name} {Menu.ToString()}";
	}
}
