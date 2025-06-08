using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public class FieldDefUser : FieldDef
{
	public FieldDefUser()
	{
	}

	public FieldDefUser(UTF8String name)
		: this(name, null)
	{
	}

	public FieldDefUser(UTF8String name, FieldSig signature)
		: this(name, signature, FieldAttributes.PrivateScope)
	{
	}

	public FieldDefUser(UTF8String name, FieldSig signature, FieldAttributes attributes)
	{
		base.name = name;
		base.signature = signature;
		base.attributes = (int)attributes;
	}
}
