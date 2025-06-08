using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Vestris.ResourceLib;

[ComVisible(true)]
public class DialogTemplate : DialogTemplateBase
{
	private User32.DIALOGTEMPLATE _header;

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
			return _header.dwExtendedStyle;
		}
		set
		{
			_header.dwExtendedStyle = value;
		}
	}

	public override ushort ControlCount => _header.cdit;

	internal override IntPtr Read(IntPtr lpRes)
	{
		_header = (User32.DIALOGTEMPLATE)Marshal.PtrToStructure(lpRes, typeof(User32.DIALOGTEMPLATE));
		lpRes = new IntPtr(lpRes.ToInt64() + 18);
		lpRes = base.Read(lpRes);
		if ((Style & 0x40u) != 0 || (Style & 0x48u) != 0)
		{
			base.TypeFace = Marshal.PtrToStringUni(lpRes);
			lpRes = new IntPtr(lpRes.ToInt64() + (base.TypeFace.Length + 1) * Marshal.SystemDefaultCharSize);
		}
		return ReadControls(lpRes);
	}

	internal override IntPtr AddControl(IntPtr lpRes)
	{
		DialogTemplateControl dialogTemplateControl = new DialogTemplateControl();
		base.Controls.Add(dialogTemplateControl);
		return dialogTemplateControl.Read(lpRes);
	}

	public override void Write(BinaryWriter w)
	{
		w.Write(_header.style);
		w.Write(_header.dwExtendedStyle);
		w.Write((ushort)base.Controls.Count);
		w.Write(_header.x);
		w.Write(_header.y);
		w.Write(_header.cx);
		w.Write(_header.cy);
		base.Write(w);
		if ((Style & 0x40u) != 0 || (Style & 0x48u) != 0)
		{
			w.Write(Encoding.Unicode.GetBytes(base.TypeFace));
			w.Write((ushort)0);
		}
		WriteControls(w);
	}

	public override string ToString()
	{
		return $"DIALOG {base.ToString()}";
	}
}
