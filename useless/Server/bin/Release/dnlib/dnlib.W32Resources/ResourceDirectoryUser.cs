using dnlib.Utils;

namespace dnlib.W32Resources;

public class ResourceDirectoryUser : ResourceDirectory
{
	public ResourceDirectoryUser(ResourceName name)
		: base(name)
	{
		directories = new LazyList<ResourceDirectory>();
		data = new LazyList<ResourceData>();
	}
}
