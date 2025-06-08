using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public class TypeRefUser : TypeRef
{
	public TypeRefUser(ModuleDef module, UTF8String name)
		: this(module, UTF8String.Empty, name)
	{
	}

	public TypeRefUser(ModuleDef module, UTF8String @namespace, UTF8String name)
		: this(module, @namespace, name, null)
	{
	}

	public TypeRefUser(ModuleDef module, UTF8String @namespace, UTF8String name, IResolutionScope resolutionScope)
	{
		base.module = module;
		base.resolutionScope = resolutionScope;
		resolutionScope_isInitialized = true;
		base.name = name;
		base.@namespace = @namespace;
	}
}
