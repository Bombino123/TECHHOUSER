using System.Threading;

namespace dnlib.W32Resources;

public class Win32ResourcesUser : Win32Resources
{
	private ResourceDirectory root = new ResourceDirectoryUser(new ResourceName("root"));

	public override ResourceDirectory Root
	{
		get
		{
			return root;
		}
		set
		{
			Interlocked.Exchange(ref root, value);
		}
	}
}
