using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Vestris.ResourceLib;

public class ResourceInfo : IEnumerable<Resource>, IEnumerable, IDisposable
{
	private Exception _innerException;

	private IntPtr _hModule = IntPtr.Zero;

	private Dictionary<ResourceId, List<Resource>> _resources;

	private List<ResourceId> _resourceTypes;

	public Dictionary<ResourceId, List<Resource>> Resources => _resources;

	public List<ResourceId> ResourceTypes => _resourceTypes;

	public List<Resource> this[Kernel32.ResourceTypes type]
	{
		get
		{
			return _resources[new ResourceId(type)];
		}
		set
		{
			_resources[new ResourceId(type)] = value;
		}
	}

	public List<Resource> this[string type]
	{
		get
		{
			return _resources[new ResourceId(type)];
		}
		set
		{
			_resources[new ResourceId(type)] = value;
		}
	}

	public void Unload()
	{
		if (_hModule != IntPtr.Zero)
		{
			Kernel32.FreeLibrary(_hModule);
			_hModule = IntPtr.Zero;
		}
		_innerException = null;
	}

	public void Load(string filename)
	{
		Unload();
		_resourceTypes = new List<ResourceId>();
		_resources = new Dictionary<ResourceId, List<Resource>>();
		_hModule = Kernel32.LoadLibraryEx(filename, IntPtr.Zero, 3u);
		if (IntPtr.Zero == _hModule)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		try
		{
			if (!Kernel32.EnumResourceTypes(_hModule, EnumResourceTypesImpl, IntPtr.Zero))
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
		}
		catch (Exception outerException)
		{
			throw new LoadException($"Error loading '{filename}'.", _innerException, outerException);
		}
	}

	private bool EnumResourceTypesImpl(IntPtr hModule, IntPtr lpszType, IntPtr lParam)
	{
		ResourceId item = new ResourceId(lpszType);
		_resourceTypes.Add(item);
		if (!Kernel32.EnumResourceNames(hModule, lpszType, EnumResourceNamesImpl, IntPtr.Zero))
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		return true;
	}

	private bool EnumResourceNamesImpl(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, IntPtr lParam)
	{
		if (!Kernel32.EnumResourceLanguages(hModule, lpszType, lpszName, EnumResourceLanguages, IntPtr.Zero))
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		return true;
	}

	protected Resource CreateResource(IntPtr hModule, IntPtr hResourceGlobal, ResourceId type, ResourceId name, ushort wIDLanguage, int size)
	{
		if (type.IsIntResource())
		{
			switch (type.ResourceType)
			{
			case Kernel32.ResourceTypes.RT_VERSION:
				return new VersionResource(hModule, hResourceGlobal, type, name, wIDLanguage, size);
			case Kernel32.ResourceTypes.RT_GROUP_CURSOR:
				return new CursorDirectoryResource(hModule, hResourceGlobal, type, name, wIDLanguage, size);
			case Kernel32.ResourceTypes.RT_GROUP_ICON:
				return new IconDirectoryResource(hModule, hResourceGlobal, type, name, wIDLanguage, size);
			case Kernel32.ResourceTypes.RT_MANIFEST:
				return new ManifestResource(hModule, hResourceGlobal, type, name, wIDLanguage, size);
			case Kernel32.ResourceTypes.RT_BITMAP:
				return new BitmapResource(hModule, hResourceGlobal, type, name, wIDLanguage, size);
			case Kernel32.ResourceTypes.RT_MENU:
				return new MenuResource(hModule, hResourceGlobal, type, name, wIDLanguage, size);
			case Kernel32.ResourceTypes.RT_DIALOG:
				return new DialogResource(hModule, hResourceGlobal, type, name, wIDLanguage, size);
			case Kernel32.ResourceTypes.RT_STRING:
				return new StringResource(hModule, hResourceGlobal, type, name, wIDLanguage, size);
			case Kernel32.ResourceTypes.RT_FONTDIR:
				return new FontDirectoryResource(hModule, hResourceGlobal, type, name, wIDLanguage, size);
			case Kernel32.ResourceTypes.RT_FONT:
				return new FontResource(hModule, hResourceGlobal, type, name, wIDLanguage, size);
			case Kernel32.ResourceTypes.RT_ACCELERATOR:
				return new AcceleratorResource(hModule, hResourceGlobal, type, name, wIDLanguage, size);
			}
		}
		return new GenericResource(hModule, hResourceGlobal, type, name, wIDLanguage, size);
	}

	private bool EnumResourceLanguages(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, ushort wIDLanguage, IntPtr lParam)
	{
		List<Resource> value = null;
		ResourceId resourceId = new ResourceId(lpszType);
		if (!_resources.TryGetValue(resourceId, out value))
		{
			value = new List<Resource>();
			_resources[resourceId] = value;
		}
		ResourceId resourceId2 = new ResourceId(lpszName);
		IntPtr intPtr = Kernel32.FindResourceEx(hModule, lpszType, lpszName, wIDLanguage);
		IntPtr lp = Kernel32.LoadResource(hModule, intPtr);
		int size = Kernel32.SizeofResource(hModule, intPtr);
		using (Aligned aligned = new Aligned(lp, size))
		{
			try
			{
				value.Add(CreateResource(hModule, aligned.Ptr, resourceId, resourceId2, wIDLanguage, size));
			}
			catch (Exception ex)
			{
				_innerException = new Exception($"Error loading resource '{resourceId2}' {resourceId.TypeName} ({wIDLanguage}).", ex);
				throw ex;
			}
		}
		return true;
	}

	public void Save(string filename)
	{
		throw new NotImplementedException();
	}

	public void Dispose()
	{
		Unload();
	}

	public IEnumerator<Resource> GetEnumerator()
	{
		Dictionary<ResourceId, List<Resource>>.Enumerator resourceTypesEnumerator = _resources.GetEnumerator();
		while (resourceTypesEnumerator.MoveNext())
		{
			List<Resource>.Enumerator resourceEnumerator = resourceTypesEnumerator.Current.Value.GetEnumerator();
			while (resourceEnumerator.MoveNext())
			{
				yield return resourceEnumerator.Current;
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
