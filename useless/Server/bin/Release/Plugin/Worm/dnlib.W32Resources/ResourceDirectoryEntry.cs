using System.Runtime.InteropServices;

namespace dnlib.W32Resources;

[ComVisible(true)]
public abstract class ResourceDirectoryEntry
{
	private ResourceName name;

	public ResourceName Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	protected ResourceDirectoryEntry(ResourceName name)
	{
		this.name = name;
	}

	public override string ToString()
	{
		return name.ToString();
	}
}
