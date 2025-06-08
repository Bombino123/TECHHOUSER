using System;

namespace dnlib.DotNet;

public class AssemblyLinkedResource : Resource
{
	private AssemblyRef asmRef;

	public override ResourceType ResourceType => ResourceType.AssemblyLinked;

	public AssemblyRef Assembly
	{
		get
		{
			return asmRef;
		}
		set
		{
			asmRef = value ?? throw new ArgumentNullException("value");
		}
	}

	public AssemblyLinkedResource(UTF8String name, AssemblyRef asmRef, ManifestResourceAttributes flags)
		: base(name, flags)
	{
		this.asmRef = asmRef ?? throw new ArgumentNullException("asmRef");
	}

	public override string ToString()
	{
		return UTF8String.ToSystemStringOrEmpty(base.Name) + " - assembly: " + asmRef.FullName;
	}
}
