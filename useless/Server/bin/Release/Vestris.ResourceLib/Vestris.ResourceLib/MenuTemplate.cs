using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Vestris.ResourceLib;

public class MenuTemplate : MenuTemplateBase
{
	private User32.MENUTEMPLATE _header;

	private MenuTemplateItemCollection _menuItems = new MenuTemplateItemCollection();

	public MenuTemplateItemCollection MenuItems
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
		_header = (User32.MENUTEMPLATE)Marshal.PtrToStructure(lpRes, typeof(User32.MENUTEMPLATE));
		IntPtr lpRes2 = new IntPtr(lpRes.ToInt64() + Marshal.SizeOf((object)_header) + _header.wOffset);
		return _menuItems.Read(lpRes2);
	}

	internal override void Write(BinaryWriter w)
	{
		w.Write(_header.wVersion);
		w.Write(_header.wOffset);
		ResourceUtil.Pad(w, _header.wOffset);
		_menuItems.Write(w);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("MENU");
		stringBuilder.Append(_menuItems.ToString());
		return stringBuilder.ToString();
	}
}
