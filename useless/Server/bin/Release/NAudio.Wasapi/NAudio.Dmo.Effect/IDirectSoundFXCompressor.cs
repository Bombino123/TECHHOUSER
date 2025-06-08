using System.Runtime.InteropServices;
using System.Security;

namespace NAudio.Dmo.Effect;

[ComImport]
[SuppressUnmanagedCodeSecurity]
[Guid("4bbd1154-62f6-4e2c-a15c-d3b6c417f7a0")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDirectSoundFXCompressor
{
	[PreserveSig]
	int SetAllParameters([In] ref DsFxCompressor param);

	[PreserveSig]
	int GetAllParameters(out DsFxCompressor param);
}
