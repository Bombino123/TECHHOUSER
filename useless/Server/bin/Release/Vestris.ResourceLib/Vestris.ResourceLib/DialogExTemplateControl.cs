using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Vestris.ResourceLib;

public class DialogExTemplateControl : DialogTemplateControlBase
{
	private User32.DIALOGEXITEMTEMPLATE _header;

	public override short x
	{
		get
		{
			return _header.x;
		}
		set
		{
			_header.x = value;
		}
	}

	public override short y
	{
		get
		{
			return _header.y;
		}
		set
		{
			_header.y = value;
		}
	}

	public override short cx
	{
		get
		{
			return _header.cx;
		}
		set
		{
			_header.cx = value;
		}
	}

	public override short cy
	{
		get
		{
			return _header.cy;
		}
		set
		{
			_header.cy = value;
		}
	}

	public override uint Style
	{
		get
		{
			return _header.style;
		}
		set
		{
			_header.style = value;
		}
	}

	public override uint ExtendedStyle
	{
		get
		{
			return _header.exStyle;
		}
		set
		{
			_header.exStyle = value;
		}
	}

	public int Id
	{
		get
		{
			return _header.id;
		}
		set
		{
			_header.id = value;
		}
	}

	internal override IntPtr Read(IntPtr lpRes)
	{
		_header = (User32.DIALOGEXITEMTEMPLATE)Marshal.PtrToStructure(lpRes, typeof(User32.DIALOGEXITEMTEMPLATE));
		lpRes = new IntPtr(lpRes.ToInt64() + Marshal.SizeOf((object)_header));
		return base.Read(lpRes);
	}

	public override void Write(BinaryWriter w)
	{
		w.Write(_header.helpID);
		w.Write(_header.exStyle);
		w.Write(_header.style);
		w.Write(_header.x);
		w.Write(_header.y);
		w.Write(_header.cx);
		w.Write(_header.cy);
		w.Write(_header.id);
		base.Write(w);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("{0} \"{1}\" {2}, {3}, {4}, {5}, {6}, {7}, {8}", base.ControlClassId, base.CaptionId, Id, base.ControlClassId, x, y, cx, cy, DialogTemplateUtil.StyleToString<User32.WindowStyles, User32.StaticControlStyles>(Style, ExtendedStyle));
		if (base.ControlClassId.IsIntResource())
		{
			switch (base.ControlClass)
			{
			case User32.DialogItemClass.Button:
				stringBuilder.AppendFormat("| {0}", (User32.ButtonControlStyles)(Style & 0xFFFFu));
				break;
			case User32.DialogItemClass.Edit:
				stringBuilder.AppendFormat("| {0}", DialogTemplateUtil.StyleToString<User32.EditControlStyles>(Style & 0xFFFFu));
				break;
			}
		}
		return stringBuilder.ToString();
	}
}
