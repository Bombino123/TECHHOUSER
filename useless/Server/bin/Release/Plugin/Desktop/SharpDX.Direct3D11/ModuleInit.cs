namespace SharpDX.Direct3D11;

internal class ModuleInit
{
	[Tag("SharpDX.ModuleInit")]
	internal static void Setup()
	{
		ResultDescriptor.RegisterProvider(typeof(ResultCode));
	}
}
