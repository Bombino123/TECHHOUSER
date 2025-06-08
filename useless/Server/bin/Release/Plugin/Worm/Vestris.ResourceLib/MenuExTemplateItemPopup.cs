using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Vestris.ResourceLib;

[ComVisible(true)]
public class MenuExTemplateItemPopup : MenuExTemplateItem
{
	private uint _dwHelpId;

	private MenuExTemplateItemCollection _subMenuItems = new MenuExTemplateItemCollection();

	public MenuExTemplateItemCollection SubMenuItems
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
		lpRes = base.Read(lpRes);
		lpRes = ResourceUtil.Align(lpRes);
		_dwHelpId = (uint)Marshal.ReadInt32(lpRes);
		lpRes = new IntPtr(lpRes.ToInt64() + 4);
		return _subMenuItems.Read(lpRes);
	}

	internal override void Write(BinaryWriter w)
	{
		base.Write(w);
		ResourceUtil.PadToDWORD(w);
		w.Write(_dwHelpId);
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
