using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Vestris.ResourceLib;

[ComVisible(true)]
public abstract class MenuTemplateItem
{
	protected User32.MENUITEMTEMPLATE _header;

	protected string _menuString;

	public string MenuString
	{
		get
		{
			return _menuString;
		}
		set
		{
			_menuString = value;
		}
	}

	internal virtual IntPtr Read(IntPtr lpRes)
	{
		if ((ushort)Marshal.ReadInt16(lpRes) == 0)
		{
			lpRes = new IntPtr(lpRes.ToInt64() + 2);
		}
		else
		{
			_menuString = Marshal.PtrToStringUni(lpRes);
			lpRes = new IntPtr(lpRes.ToInt64() + (_menuString.Length + 1) * Marshal.SystemDefaultCharSize);
		}
		return lpRes;
	}

	internal virtual void Write(BinaryWriter w)
	{
		if (_menuString == null)
		{
			w.Write((ushort)0);
			return;
		}
		w.Write(Encoding.Unicode.GetBytes(_menuString));
		w.Write((ushort)0);
		ResourceUtil.PadToWORD(w);
	}

	public abstract string ToString(int indent);

	public override string ToString()
	{
		return ToString(0);
	}
}
