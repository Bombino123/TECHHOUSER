using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public class EventDefUser : EventDef
{
	public EventDefUser()
	{
	}

	public EventDefUser(UTF8String name)
		: this(name, null, (EventAttributes)0)
	{
	}

	public EventDefUser(UTF8String name, ITypeDefOrRef type)
		: this(name, type, (EventAttributes)0)
	{
	}

	public EventDefUser(UTF8String name, ITypeDefOrRef type, EventAttributes flags)
	{
		base.name = name;
		eventType = type;
		attributes = (int)flags;
	}
}
