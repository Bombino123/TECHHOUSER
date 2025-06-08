using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public class ModuleRefUser : ModuleRef
{
	public ModuleRefUser(ModuleDef module)
		: this(module, UTF8String.Empty)
	{
	}

	public ModuleRefUser(ModuleDef module, UTF8String name)
	{
		base.module = module;
		base.name = name;
	}
}
