using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Vestris.ResourceLib;

[ComVisible(true)]
public class FontDirectoryEntry
{
	private ushort _fontOrdinal;

	private User32.FONTDIRENTRY _font;

	private string _faceName;

	private string _deviceName;

	public ushort FontOrdinal
	{
		get
		{
			return _fontOrdinal;
		}
		set
		{
			_fontOrdinal = value;
		}
	}

	public string FaceName
	{
		get
		{
			return _faceName;
		}
		set
		{
			_faceName = value;
		}
	}

	public string DeviceName
	{
		get
		{
			return _deviceName;
		}
		set
		{
			_deviceName = value;
		}
	}

	public User32.FONTDIRENTRY Font
	{
		get
		{
			return _font;
		}
		set
		{
			_font = value;
		}
	}

	internal IntPtr Read(IntPtr lpRes)
	{
		_fontOrdinal = (ushort)Marshal.ReadInt16(lpRes);
		lpRes = new IntPtr(lpRes.ToInt64() + 2);
		_font = (User32.FONTDIRENTRY)Marshal.PtrToStructure(lpRes, typeof(User32.FONTDIRENTRY));
		lpRes = new IntPtr(lpRes.ToInt64() + Marshal.SizeOf((object)_font));
		_deviceName = Marshal.PtrToStringAnsi(lpRes);
		lpRes = new IntPtr(lpRes.ToInt64() + _deviceName.Length + 1);
		_faceName = Marshal.PtrToStringAnsi(lpRes);
		lpRes = new IntPtr(lpRes.ToInt64() + _faceName.Length + 1);
		return lpRes;
	}

	public void Write(BinaryWriter w)
	{
		w.Write(_fontOrdinal);
		w.Write(ResourceUtil.GetBytes(_font));
		if (!string.IsNullOrEmpty(_deviceName))
		{
			w.Write(Encoding.ASCII.GetBytes(_deviceName));
		}
		w.Write((byte)0);
		if (!string.IsNullOrEmpty(_faceName))
		{
			w.Write(Encoding.ASCII.GetBytes(_faceName));
		}
		w.Write((byte)0);
	}
}
