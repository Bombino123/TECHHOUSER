using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public class ParamDefUser : ParamDef
{
	public ParamDefUser()
	{
	}

	public ParamDefUser(UTF8String name)
		: this(name, 0)
	{
	}

	public ParamDefUser(UTF8String name, ushort sequence)
		: this(name, sequence, (ParamAttributes)0)
	{
	}

	public ParamDefUser(UTF8String name, ushort sequence, ParamAttributes flags)
	{
		base.name = name;
		base.sequence = sequence;
		attributes = (int)flags;
	}
}
