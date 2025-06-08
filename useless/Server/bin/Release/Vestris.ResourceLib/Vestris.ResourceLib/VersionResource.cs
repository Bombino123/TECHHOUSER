using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace Vestris.ResourceLib;

public class VersionResource : Resource
{
	private ResourceTableHeader _header = new ResourceTableHeader("VS_VERSION_INFO");

	private FixedFileInfo _fixedfileinfo = new FixedFileInfo();

	private OrderedDictionary _resources = new OrderedDictionary();

	public ResourceTableHeader Header => _header;

	public OrderedDictionary Resources => _resources;

	public string FileVersion
	{
		get
		{
			return _fixedfileinfo.FileVersion;
		}
		set
		{
			_fixedfileinfo.FileVersion = value;
		}
	}

	public uint FileFlags
	{
		get
		{
			return _fixedfileinfo.FileFlags;
		}
		set
		{
			_fixedfileinfo.FileFlags = value;
		}
	}

	public string ProductVersion
	{
		get
		{
			return _fixedfileinfo.ProductVersion;
		}
		set
		{
			_fixedfileinfo.ProductVersion = value;
		}
	}

	public ResourceTableHeader this[string key]
	{
		get
		{
			return (ResourceTableHeader)Resources[key];
		}
		set
		{
			Resources[key] = value;
		}
	}

	public ResourceTableHeader this[int index]
	{
		get
		{
			return (ResourceTableHeader)Resources[index];
		}
		set
		{
			Resources[index] = value;
		}
	}

	public VersionResource(IntPtr hModule, IntPtr hResource, ResourceId type, ResourceId name, ushort language, int size)
		: base(hModule, hResource, type, name, language, size)
	{
	}

	public VersionResource()
		: base(IntPtr.Zero, IntPtr.Zero, new ResourceId(Kernel32.ResourceTypes.RT_VERSION), new ResourceId(1u), ResourceUtil.USENGLISHLANGID, 0)
	{
		_header.Header = new Kernel32.RESOURCE_HEADER(_fixedfileinfo.Size);
	}

	internal override IntPtr Read(IntPtr hModule, IntPtr lpRes)
	{
		_resources.Clear();
		IntPtr lpRes2 = _header.Read(lpRes);
		if (_header.Header.wValueLength != 0)
		{
			_fixedfileinfo = new FixedFileInfo();
			_fixedfileinfo.Read(lpRes2);
		}
		IntPtr lpRes3 = ResourceUtil.Align(lpRes2.ToInt64() + _header.Header.wValueLength);
		while (lpRes3.ToInt64() < lpRes.ToInt64() + _header.Header.wLength)
		{
			ResourceTableHeader resourceTableHeader = new ResourceTableHeader(lpRes3);
			string key = resourceTableHeader.Key;
			resourceTableHeader = ((!(key == "StringFileInfo")) ? ((ResourceTableHeader)new VarFileInfo(lpRes3)) : ((ResourceTableHeader)new StringFileInfo(lpRes3)));
			_resources.Add(resourceTableHeader.Key, resourceTableHeader);
			lpRes3 = ResourceUtil.Align(lpRes3.ToInt64() + resourceTableHeader.Header.wLength);
		}
		return new IntPtr(lpRes.ToInt64() + _header.Header.wLength);
	}

	internal override void Write(BinaryWriter w)
	{
		long position = w.BaseStream.Position;
		_header.Write(w);
		if (_fixedfileinfo != null)
		{
			_fixedfileinfo.Write(w);
		}
		foreach (DictionaryEntry resource in _resources)
		{
			((ResourceTableHeader)resource.Value).Write(w);
		}
		ResourceUtil.WriteAt(w, w.BaseStream.Position - position, position);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (_fixedfileinfo != null)
		{
			stringBuilder.Append(_fixedfileinfo.ToString());
		}
		stringBuilder.AppendLine("BEGIN");
		foreach (DictionaryEntry resource in _resources)
		{
			stringBuilder.Append(((ResourceTableHeader)resource.Value).ToString(1));
		}
		stringBuilder.AppendLine("END");
		return stringBuilder.ToString();
	}
}
