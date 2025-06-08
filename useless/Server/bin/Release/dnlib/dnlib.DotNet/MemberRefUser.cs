namespace dnlib.DotNet;

public class MemberRefUser : MemberRef
{
	public MemberRefUser(ModuleDef module)
	{
		base.module = module;
	}

	public MemberRefUser(ModuleDef module, UTF8String name)
	{
		base.module = module;
		base.name = name;
	}

	public MemberRefUser(ModuleDef module, UTF8String name, FieldSig sig)
		: this(module, name, sig, null)
	{
	}

	public MemberRefUser(ModuleDef module, UTF8String name, FieldSig sig, IMemberRefParent @class)
	{
		base.module = module;
		base.name = name;
		base.@class = @class;
		signature = sig;
	}

	public MemberRefUser(ModuleDef module, UTF8String name, MethodSig sig)
		: this(module, name, sig, null)
	{
	}

	public MemberRefUser(ModuleDef module, UTF8String name, MethodSig sig, IMemberRefParent @class)
	{
		base.module = module;
		base.name = name;
		base.@class = @class;
		signature = sig;
	}
}
