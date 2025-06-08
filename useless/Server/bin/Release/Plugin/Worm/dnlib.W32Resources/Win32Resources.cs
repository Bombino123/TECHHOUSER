using System;
using System.Runtime.InteropServices;

namespace dnlib.W32Resources;

[ComVisible(true)]
public abstract class Win32Resources : IDisposable
{
	public abstract ResourceDirectory Root { get; set; }

	public ResourceDirectory Find(ResourceName type)
	{
		return Root?.FindDirectory(type);
	}

	public ResourceDirectory Find(ResourceName type, ResourceName name)
	{
		return Find(type)?.FindDirectory(name);
	}

	public ResourceData Find(ResourceName type, ResourceName name, ResourceName langId)
	{
		return Find(type, name)?.FindData(langId);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			Root = null;
		}
	}
}
