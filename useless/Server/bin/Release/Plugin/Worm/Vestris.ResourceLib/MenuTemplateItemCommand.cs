using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Vestris.ResourceLib;

[ComVisible(true)]
public class MenuTemplateItemCommand : MenuTemplateItem
{
	private ushort _menuId;

	public ushort MenuId
	{
		get
		{
			return _menuId;
		}
		set
		{
			_menuId = value;
		}
	}

	public bool IsSeparator
	{
		get
		{
			if ((_header.mtOption & 0x800) == 0)
			{
				if (_header.mtOption == 0 && _menuString == null)
				{
					return _menuId == 0;
				}
				return false;
			}
			return true;
		}
	}

	internal override IntPtr Read(IntPtr lpRes)
	{
		_header = (User32.MENUITEMTEMPLATE)Marshal.PtrToStructure(lpRes, typeof(User32.MENUITEMTEMPLATE));
		lpRes = new IntPtr(lpRes.ToInt64() + Marshal.SizeOf((object)_header));
		_menuId = (ushort)Marshal.ReadInt16(lpRes);
		lpRes = new IntPtr(lpRes.ToInt64() + 2);
		lpRes = base.Read(lpRes);
		return lpRes;
	}

	internal override void Write(BinaryWriter w)
	{
		w.Write(_header.mtOption);
		w.Write(_menuId);
		base.Write(w);
	}

	public override string ToString(int indent)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (IsSeparator)
		{
			stringBuilder.AppendLine($"{new string(' ', indent)}MENUITEM SEPARATOR");
		}
		else
		{
			stringBuilder.AppendLine(string.Format("{0}MENUITEM \"{1}\", {2}", new string(' ', indent), (_menuString == null) ? string.Empty : _menuString.Replace("\t", "\\t"), _menuId));
		}
		return stringBuilder.ToString();
	}
}
