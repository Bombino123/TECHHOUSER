using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Vestris.ResourceLib;

[ComVisible(true)]
public abstract class DialogTemplateBase
{
	private string _caption;

	private ResourceId _menuId;

	private ResourceId _windowClassId;

	private ushort _pointSize;

	private string _typeface;

	private List<DialogTemplateControlBase> _controls = new List<DialogTemplateControlBase>();

	public abstract short x { get; set; }

	public abstract short y { get; set; }

	public abstract short cx { get; set; }

	public abstract short cy { get; set; }

	public abstract uint Style { get; set; }

	public abstract uint ExtendedStyle { get; set; }

	public abstract ushort ControlCount { get; }

	public string TypeFace
	{
		get
		{
			return _typeface;
		}
		set
		{
			_typeface = value;
		}
	}

	public ushort PointSize
	{
		get
		{
			return _pointSize;
		}
		set
		{
			_pointSize = value;
		}
	}

	public string Caption
	{
		get
		{
			return _caption;
		}
		set
		{
			_caption = value;
		}
	}

	public ResourceId MenuId
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

	public ResourceId WindowClassId
	{
		get
		{
			return _windowClassId;
		}
		set
		{
			_windowClassId = value;
		}
	}

	public List<DialogTemplateControlBase> Controls
	{
		get
		{
			return _controls;
		}
		set
		{
			_controls = value;
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"{x}, {y}, {x + cx}, {y + cy}");
		string text = DialogTemplateUtil.StyleToString<User32.WindowStyles, User32.DialogStyles>(Style);
		if (!string.IsNullOrEmpty(text))
		{
			stringBuilder.AppendLine("STYLE " + text);
		}
		string text2 = DialogTemplateUtil.StyleToString<User32.WindowStyles, User32.ExtendedDialogStyles>(ExtendedStyle);
		if (!string.IsNullOrEmpty(text2))
		{
			stringBuilder.AppendLine("EXSTYLE " + text2);
		}
		stringBuilder.AppendLine($"CAPTION \"{_caption}\"");
		stringBuilder.AppendLine($"FONT {_pointSize}, \"{_typeface}\"");
		if (_controls.Count > 0)
		{
			stringBuilder.AppendLine("{");
			foreach (DialogTemplateControlBase control in _controls)
			{
				stringBuilder.AppendLine(" " + control.ToString());
			}
			stringBuilder.AppendLine("}");
		}
		return stringBuilder.ToString();
	}

	public virtual string ToControlString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("{0} \"{1}\" {2}, {3}, {4}, {5}", WindowClassId, Caption, x, y, cx, cy);
		return stringBuilder.ToString();
	}

	internal virtual IntPtr Read(IntPtr lpRes)
	{
		lpRes = DialogTemplateUtil.ReadResourceId(lpRes, out _menuId);
		lpRes = DialogTemplateUtil.ReadResourceId(lpRes, out _windowClassId);
		Caption = Marshal.PtrToStringUni(lpRes);
		lpRes = new IntPtr(lpRes.ToInt64() + (Caption.Length + 1) * Marshal.SystemDefaultCharSize);
		if ((Style & 0x40u) != 0 || (Style & 0x48u) != 0)
		{
			PointSize = (ushort)Marshal.ReadInt16(lpRes);
			lpRes = new IntPtr(lpRes.ToInt64() + 2);
		}
		return lpRes;
	}

	internal abstract IntPtr AddControl(IntPtr lpRes);

	internal IntPtr ReadControls(IntPtr lpRes)
	{
		for (int i = 0; i < ControlCount; i++)
		{
			lpRes = ResourceUtil.Align(lpRes);
			lpRes = AddControl(lpRes);
		}
		return lpRes;
	}

	internal void WriteControls(BinaryWriter w)
	{
		foreach (DialogTemplateControlBase control in Controls)
		{
			ResourceUtil.PadToDWORD(w);
			control.Write(w);
		}
	}

	public virtual void Write(BinaryWriter w)
	{
		DialogTemplateUtil.WriteResourceId(w, _menuId);
		DialogTemplateUtil.WriteResourceId(w, _windowClassId);
		w.Write(Encoding.Unicode.GetBytes(Caption));
		w.Write((ushort)0);
		if ((Style & 0x40u) != 0 || (Style & 0x48u) != 0)
		{
			w.Write(PointSize);
		}
	}
}
