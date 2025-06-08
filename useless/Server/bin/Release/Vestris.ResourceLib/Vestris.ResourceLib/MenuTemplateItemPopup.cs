using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Vestris.ResourceLib;

public class MenuTemplateItemPopup : MenuTemplateItem
{
	private MenuTemplateItemCollection _subMenuItems = new MenuTemplateItemCollection();

	public MenuTemplateItemCollection SubMenuItems
	{
		get
		{
			return _subMenuItems;
		}
		set
		{
			_subMenuItems = value;
		}
	}

	internal override IntPtr Read(IntPtr lpRes)
	{
		_header = (User32.MENUITEMTEMPLATE)Marshal.PtrToStructure(lpRes, typeof(User32.MENUITEMTEMPLATE));
		lpRes = new IntPtr(lpRes.ToInt64() + Marshal.SizeOf((object)_header));
		lpRes = base.Read(lpRes);
		return _subMenuItems.Read(lpRes);
	}

	internal override void Write(BinaryWriter w)
	{
		w.Write(_header.mtOption);
		base.Write(w);
		_subMenuItems.Write(w);
	}

	public override string ToString(int indent)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(string.Format("{0}POPUP \"{1}\"", new string(' ', indent), (_menuString == null) ? string.Empty : _menuString.Replace("\t", "\\t")));
		stringBuilder.Append(_subMenuItems.ToString(indent));
		return stringBuilder.ToString();
	}
}
