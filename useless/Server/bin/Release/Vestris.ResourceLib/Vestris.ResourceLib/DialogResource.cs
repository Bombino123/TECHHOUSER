using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Vestris.ResourceLib;

public class DialogResource : Resource
{
	private DialogTemplateBase _dlgtemplate;

	public DialogTemplateBase Template
	{
		get
		{
			return _dlgtemplate;
		}
		set
		{
			_dlgtemplate = value;
		}
	}

	public DialogResource(IntPtr hModule, IntPtr hResource, ResourceId type, ResourceId name, ushort language, int size)
		: base(hModule, hResource, type, name, language, size)
	{
	}

	public DialogResource()
		: base(IntPtr.Zero, IntPtr.Zero, new ResourceId(Kernel32.ResourceTypes.RT_DIALOG), new ResourceId(1u), ResourceUtil.NEUTRALLANGID, 0)
	{
	}

	internal override IntPtr Read(IntPtr hModule, IntPtr lpRes)
	{
		uint num = (uint)Marshal.ReadInt32(lpRes) >> 16;
		if (num == 65535)
		{
			_dlgtemplate = new DialogExTemplate();
		}
		else
		{
			_dlgtemplate = new DialogTemplate();
		}
		return _dlgtemplate.Read(lpRes);
	}

	internal override void Write(BinaryWriter w)
	{
		_dlgtemplate.Write(w);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("{0} {1}", base.Name.IsIntResource() ? base.Name.ToString() : ("\"" + base.Name.ToString() + "\""), _dlgtemplate);
		return stringBuilder.ToString();
	}
}
