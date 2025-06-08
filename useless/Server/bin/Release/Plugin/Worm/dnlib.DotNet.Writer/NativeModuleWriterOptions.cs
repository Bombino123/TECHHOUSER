using System.Runtime.InteropServices;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public sealed class NativeModuleWriterOptions : ModuleWriterOptionsBase
{
	public bool KeepExtraPEData { get; set; }

	public bool KeepWin32Resources { get; set; }

	internal bool OptimizeImageSize { get; }

	public NativeModuleWriterOptions(ModuleDefMD module, bool optimizeImageSize)
		: base(module)
	{
		base.MetadataOptions.Flags |= MetadataFlags.PreserveAllMethodRids;
		if (optimizeImageSize)
		{
			OptimizeImageSize = true;
			base.MetadataOptions.Flags |= MetadataFlags.PreserveTypeRefRids | MetadataFlags.PreserveTypeDefRids | MetadataFlags.PreserveTypeSpecRids;
		}
	}
}
