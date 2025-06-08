using dnlib.Utils;

namespace dnlib.DotNet;

public class GenericParamUser : GenericParam
{
	public GenericParamUser()
	{
	}

	public GenericParamUser(ushort number)
		: this(number, GenericParamAttributes.NonVariant)
	{
	}

	public GenericParamUser(ushort number, GenericParamAttributes flags)
		: this(number, flags, UTF8String.Empty)
	{
	}

	public GenericParamUser(ushort number, GenericParamAttributes flags, UTF8String name)
	{
		genericParamConstraints = new LazyList<GenericParamConstraint>(this);
		base.number = number;
		attributes = (int)flags;
		base.name = name;
	}
}
