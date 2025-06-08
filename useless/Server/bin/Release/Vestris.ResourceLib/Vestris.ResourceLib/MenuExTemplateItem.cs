using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Vestris.ResourceLib;

public abstract class MenuExTemplateItem
{
	protected User32.MENUEXITEMTEMPLATE _header;

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
		_header = (User32.MENUEXITEMTEMPLATE)Marshal.PtrToStructure(lpRes, typeof(User32.MENUEXITEMTEMPLATE));
		lpRes = new IntPtr(lpRes.ToInt64() + Marshal.SizeOf((object)_header));
		if (Marshal.ReadInt32(lpRes) != 0)
		{
			_menuString = Marshal.PtrToStringUni(lpRes);
			lpRes = new IntPtr(lpRes.ToInt64() + (_menuString.Length + 1) * Marshal.SystemDefaultCharSize);
		}
		return lpRes;
	}

	internal virtual void Write(BinaryWriter w)
	{
		w.Write(_header.dwType);
		w.Write(_header.dwState);
		w.Write(_header.dwMenuId);
		w.Write(_header.bResInfo);
		if (_menuString != null)
		{
			w.Write(Encoding.Unicode.GetBytes(_menuString));
			w.Write((ushort)0);
			ResourceUtil.PadToDWORD(w);
		}
	}

	public abstract string ToString(int indent);

	public override string ToString()
	{
		return ToString(0);
	}
}
