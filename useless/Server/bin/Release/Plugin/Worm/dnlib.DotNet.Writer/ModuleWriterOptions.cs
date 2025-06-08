using System.Runtime.InteropServices;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public sealed class ModuleWriterOptions : ModuleWriterOptionsBase
{
	public ModuleWriterOptions(ModuleDef module)
		: base(module)
	{
	}
}
