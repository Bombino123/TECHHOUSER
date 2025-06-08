using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Vestris.ResourceLib;

public class MenuExTemplate : MenuTemplateBase
{
	private User32.MENUEXTEMPLATE _header;

	private MenuExTemplateItemCollection _menuItems = new MenuExTemplateItemCollection();

	public MenuExTemplateItemCollection MenuItems
	{
		get
		{
			return _menuItems;
		}
		set
		{
			_menuItems = value;
		}
	}

	internal override IntPtr Read(IntPtr lpRes)
	{
		_header = (User32.MENUEXTEMPLATE)Marshal.PtrToStructure(lpRes, typeof(User32.MENUEXTEMPLATE));
		IntPtr lpRes2 = ResourceUtil.Align(lpRes.ToInt64() + Marshal.SizeOf((object)_header) + _header.wOffset);
		return _menuItems.Read(lpRes2);
	}

	internal override void Write(BinaryWriter w)
	{
		long position = w.BaseStream.Position;
		w.Write(_header.wVersion);
		w.Write(_header.wOffset);
		ResourceUtil.Pad(w, (ushort)(_header.wOffset - 4));
		w.BaseStream.Seek(position + _header.wOffset + 4, SeekOrigin.Begin);
		_menuItems.Write(w);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("MENUEX");
		stringBuilder.Append(_menuItems.ToString());
		return stringBuilder.ToString();
	}
}
