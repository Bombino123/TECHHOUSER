using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace Vestris.ResourceLib;

[ComVisible(true)]
public class ManifestResource : Resource
{
	private static byte[] utf8_bom = new byte[3] { 239, 187, 191 };

	private byte[] _data;

	private XmlDocument _manifest;

	public XmlDocument Manifest
	{
		get
		{
			if (_manifest == null && _data != null)
			{
				bool flag = _data.Length >= 3 && _data[0] == utf8_bom[0] && _data[1] == utf8_bom[1] && _data[2] == utf8_bom[2];
				string @string = Encoding.UTF8.GetString(_data, flag ? 3 : 0, flag ? (_data.Length - 3) : _data.Length);
				_manifest = new XmlDocument();
				_manifest.LoadXml(@string);
			}
			return _manifest;
		}
		set
		{
			_manifest = value;
			_data = null;
			_size = Encoding.UTF8.GetBytes(_manifest.OuterXml).Length;
		}
	}

	public Kernel32.ManifestType ManifestType
	{
		get
		{
			return (Kernel32.ManifestType)(int)_name.Id;
		}
		set
		{
			_name = new ResourceId((IntPtr)(int)value);
		}
	}

	public ManifestResource(IntPtr hModule, IntPtr hResource, ResourceId type, ResourceId name, ushort language, int size)
		: base(hModule, hResource, type, name, language, size)
	{
	}

	public ManifestResource()
		: this(Kernel32.ManifestType.CreateProcess)
	{
	}

	public ManifestResource(Kernel32.ManifestType manifestType)
		: base(IntPtr.Zero, IntPtr.Zero, new ResourceId(Kernel32.ResourceTypes.RT_MANIFEST), new ResourceId((uint)manifestType), 0, 0)
	{
		_manifest = new XmlDocument();
		_manifest.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><assembly xmlns=\"urn:schemas-microsoft-com:asm.v1\" manifestVersion=\"1.0\" />");
		_size = Encoding.UTF8.GetBytes(_manifest.OuterXml).Length;
	}

	internal override IntPtr Read(IntPtr hModule, IntPtr lpRes)
	{
		if (_size > 0)
		{
			_manifest = null;
			_data = new byte[_size];
			Marshal.Copy(lpRes, _data, 0, _data.Length);
		}
		return new IntPtr(lpRes.ToInt64() + _size);
	}

	internal override void Write(BinaryWriter w)
	{
		if (_manifest != null)
		{
			w.Write(Encoding.UTF8.GetBytes(_manifest.OuterXml));
		}
		else if (_data != null)
		{
			w.Write(_data);
		}
	}

	public void LoadFrom(string filename, Kernel32.ManifestType manifestType)
	{
		LoadFrom(filename, new ResourceId(Kernel32.ResourceTypes.RT_MANIFEST), new ResourceId((uint)manifestType), 0);
	}
}
