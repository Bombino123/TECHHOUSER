using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Vestris.ResourceLib;

public class DialogExTemplate : DialogTemplateBase
{
	private User32.DIALOGEXTEMPLATE _header;

	private byte _characterSet;

	private ushort _weight;

	private bool _italic;

	public byte CharacterSet
	{
		get
		{
			return _characterSet;
		}
		set
		{
			_characterSet = value;
		}
	}

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

	public ushort Weight
	{
		get
		{
			return _weight;
		}
		set
		{
			_weight = value;
		}
	}

	public bool Italic
	{
		get
		{
			return _italic;
		}
		set
		{
			_italic = value;
		}
	}

	public override ushort ControlCount => _header.cDlgItems;

	internal override IntPtr Read(IntPtr lpRes)
	{
		_header = (User32.DIALOGEXTEMPLATE)Marshal.PtrToStructure(lpRes, typeof(User32.DIALOGEXTEMPLATE));
		lpRes = base.Read(new IntPtr(lpRes.ToInt64() + 26));
		if ((Style & 0x40u) != 0 || (Style & 0x48u) != 0)
		{
			Weight = (ushort)Marshal.ReadInt16(lpRes);
			lpRes = new IntPtr(lpRes.ToInt64() + 2);
			Italic = Marshal.ReadByte(lpRes) > 0;
			lpRes = new IntPtr(lpRes.ToInt64() + 1);
			CharacterSet = Marshal.ReadByte(lpRes);
			lpRes = new IntPtr(lpRes.ToInt64() + 1);
			base.TypeFace = Marshal.PtrToStringUni(lpRes);
			lpRes = new IntPtr(lpRes.ToInt64() + (base.TypeFace.Length + 1) * Marshal.SystemDefaultCharSize);
		}
		return ReadControls(lpRes);
	}

	internal override IntPtr AddControl(IntPtr lpRes)
	{
		DialogExTemplateControl dialogExTemplateControl = new DialogExTemplateControl();
		base.Controls.Add(dialogExTemplateControl);
		return dialogExTemplateControl.Read(lpRes);
	}

	public override void Write(BinaryWriter w)
	{
		w.Write(_header.dlgVer);
		w.Write(_header.signature);
		w.Write(_header.helpID);
		w.Write(_header.exStyle);
		w.Write(_header.style);
		w.Write((ushort)base.Controls.Count);
		w.Write(_header.x);
		w.Write(_header.y);
		w.Write(_header.cx);
		w.Write(_header.cy);
		base.Write(w);
		if ((Style & 0x40u) != 0 || (Style & 0x48u) != 0)
		{
			w.Write(Weight);
			w.Write((byte)(Italic ? 1u : 0u));
			w.Write(CharacterSet);
			w.Write(Encoding.Unicode.GetBytes(base.TypeFace));
			w.Write((ushort)0);
		}
		WriteControls(w);
	}

	public override string ToString()
	{
		return $"DIALOGEX {base.ToString()}";
	}
}
