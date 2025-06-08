using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public class FileDefUser : FileDef
{
	public FileDefUser()
	{
	}

	public FileDefUser(UTF8String name, FileAttributes flags, byte[] hashValue)
	{
		base.name = name;
		attributes = (int)flags;
		base.hashValue = hashValue;
	}
}
