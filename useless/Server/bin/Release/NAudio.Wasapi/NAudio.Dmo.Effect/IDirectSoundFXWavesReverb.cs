using System.Runtime.InteropServices;
using System.Security;

namespace NAudio.Dmo.Effect;

[ComImport]
[SuppressUnmanagedCodeSecurity]
[Guid("46858c3a-0dc6-45e3-b760-d4eef16cb325")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDirectSoundFXWavesReverb
{
	[PreserveSig]
	int SetAllParameters([In] ref DsFxWavesReverb param);

	[PreserveSig]
	int GetAllParameters(out DsFxWavesReverb param);
}
