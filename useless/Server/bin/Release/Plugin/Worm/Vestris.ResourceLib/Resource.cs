using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Vestris.ResourceLib;

[ComVisible(true)]
public abstract class Resource
{
	protected ResourceId _type;

	protected ResourceId _name;

	protected ushort _language;

	protected IntPtr _hModule = IntPtr.Zero;

	protected IntPtr _hResource = IntPtr.Zero;

	protected int _size;

	public int Size => _size;

	public ushort Language
	{
		get
		{
			return _language;
		}
		set
		{
			_language = value;
		}
	}

	public ResourceId Type => _type;

	public string TypeName => _type.TypeName;

	public ResourceId Name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	internal Resource()
	{
	}

	internal Resource(IntPtr hModule, IntPtr hResource, ResourceId type, ResourceId name, ushort language, int size)
	{
		_hModule = hModule;
		_type = type;
		_name = name;
		_language = language;
		_hResource = hResource;
		_size = size;
		LockAndReadResource(hModule, hResource);
	}

	internal void LockAndReadResource(IntPtr hModule, IntPtr hResource)
	{
		if (hResource == IntPtr.Zero)
		{
			return;
		}
		IntPtr intPtr = Kernel32.LockResource(hResource);
		if (intPtr == IntPtr.Zero)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		using Aligned aligned = new Aligned(intPtr, _size);
		Read(hModule, aligned.Ptr);
	}

	public virtual void LoadFrom(string filename)
	{
		LoadFrom(filename, _type, _name, _language);
	}

	internal void LoadFrom(string filename, ResourceId type, ResourceId name, ushort lang)
	{
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			intPtr = Kernel32.LoadLibraryEx(filename, IntPtr.Zero, 3u);
			LoadFrom(intPtr, type, name, lang);
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				Kernel32.FreeLibrary(intPtr);
			}
		}
	}

	internal void LoadFrom(IntPtr hModule, ResourceId type, ResourceId name, ushort lang)
	{
		if (IntPtr.Zero == hModule)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		IntPtr intPtr = Kernel32.FindResourceEx(hModule, type.Id, name.Id, lang);
		if (IntPtr.Zero == intPtr)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		IntPtr intPtr2 = Kernel32.LoadResource(hModule, intPtr);
		if (IntPtr.Zero == intPtr2)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		IntPtr intPtr3 = Kernel32.LockResource(intPtr2);
		if (intPtr3 == IntPtr.Zero)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		_size = Kernel32.SizeofResource(hModule, intPtr);
		if (_size <= 0)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		using Aligned aligned = new Aligned(intPtr3, _size);
		_type = type;
		_name = name;
		_language = lang;
		Read(hModule, aligned.Ptr);
	}

	internal abstract IntPtr Read(IntPtr hModule, IntPtr lpRes);

	internal abstract void Write(BinaryWriter w);

	public byte[] WriteAndGetBytes()
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream, Encoding.Default);
		Write(binaryWriter);
		binaryWriter.Close();
		return memoryStream.ToArray();
	}

	public virtual void SaveTo(string filename)
	{
		SaveTo(filename, _type, _name, _language);
	}

	internal void SaveTo(string filename, ResourceId type, ResourceId name, ushort langid)
	{
		byte[] data = WriteAndGetBytes();
		SaveTo(filename, type, name, langid, data);
	}

	public virtual void DeleteFrom(string filename)
	{
		Delete(filename, _type, _name, _language);
	}

	internal static void Delete(string filename, ResourceId type, ResourceId name, ushort lang)
	{
		SaveTo(filename, type, name, lang, null);
	}

	internal static void SaveTo(string filename, ResourceId type, ResourceId name, ushort lang, byte[] data)
	{
		IntPtr intPtr = Kernel32.BeginUpdateResource(filename, bDeleteExistingResources: false);
		if (intPtr == IntPtr.Zero)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		try
		{
			if (data != null && data.Length == 0)
			{
				data = null;
			}
			if (!Kernel32.UpdateResource(intPtr, type.Id, name.Id, lang, data, (data != null) ? ((uint)data.Length) : 0u))
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
		}
		catch
		{
			Kernel32.EndUpdateResource(intPtr, fDiscard: true);
			throw;
		}
		if (!Kernel32.EndUpdateResource(intPtr, fDiscard: false))
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
	}

	public static void Save(string filename, IEnumerable<Resource> resources)
	{
		IntPtr intPtr = Kernel32.BeginUpdateResource(filename, bDeleteExistingResources: false);
		if (intPtr == IntPtr.Zero)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		try
		{
			foreach (Resource resource in resources)
			{
				if (resource is IconImageResource iconImageResource)
				{
					byte[] array = ((iconImageResource.Image == null) ? null : iconImageResource.Image.Data);
					if (!Kernel32.UpdateResource(intPtr, iconImageResource.Type.Id, new IntPtr(iconImageResource.Id), iconImageResource.Language, array, (array != null) ? ((uint)array.Length) : 0u))
					{
						throw new Win32Exception(Marshal.GetLastWin32Error());
					}
					continue;
				}
				byte[] array2 = resource.WriteAndGetBytes();
				if (array2 != null && array2.Length == 0)
				{
					array2 = null;
				}
				if (!Kernel32.UpdateResource(intPtr, resource.Type.Id, resource.Name.Id, resource.Language, array2, (array2 != null) ? ((uint)array2.Length) : 0u))
				{
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}
			}
		}
		catch
		{
			Kernel32.EndUpdateResource(intPtr, fDiscard: true);
			throw;
		}
		if (!Kernel32.EndUpdateResource(intPtr, fDiscard: false))
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
	}
}
