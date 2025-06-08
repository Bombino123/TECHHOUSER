using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public class LinkedResource : Resource
{
	private FileDef file;

	public override ResourceType ResourceType => ResourceType.Linked;

	public FileDef File
	{
		get
		{
			return file;
		}
		set
		{
			file = value ?? throw new ArgumentNullException("value");
		}
	}

	public byte[] Hash
	{
		get
		{
			return file.HashValue;
		}
		set
		{
			file.HashValue = value;
		}
	}

	public UTF8String FileName
	{
		get
		{
			if (file != null)
			{
				return file.Name;
			}
			return UTF8String.Empty;
		}
	}

	public LinkedResource(UTF8String name, FileDef file, ManifestResourceAttributes flags)
		: base(name, flags)
	{
		this.file = file;
	}

	public override string ToString()
	{
		return UTF8String.ToSystemStringOrEmpty(base.Name) + " - file: " + UTF8String.ToSystemStringOrEmpty(FileName);
	}
}
