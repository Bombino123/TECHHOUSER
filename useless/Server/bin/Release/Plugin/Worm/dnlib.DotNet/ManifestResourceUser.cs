using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public class ManifestResourceUser : ManifestResource
{
	public ManifestResourceUser()
	{
	}

	public ManifestResourceUser(UTF8String name, IImplementation implementation)
		: this(name, implementation, (ManifestResourceAttributes)0u)
	{
	}

	public ManifestResourceUser(UTF8String name, IImplementation implementation, ManifestResourceAttributes flags)
		: this(name, implementation, flags, 0u)
	{
	}

	public ManifestResourceUser(UTF8String name, IImplementation implementation, ManifestResourceAttributes flags, uint offset)
	{
		base.name = name;
		base.implementation = implementation;
		attributes = (int)flags;
		base.offset = offset;
	}
}
