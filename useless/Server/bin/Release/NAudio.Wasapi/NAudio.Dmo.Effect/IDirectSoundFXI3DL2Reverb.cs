using System.Runtime.InteropServices;
using System.Security;

namespace NAudio.Dmo.Effect;

[ComImport]
[SuppressUnmanagedCodeSecurity]
[Guid("4b166a6a-0d66-43f3-80e3-ee6280dee1a4")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDirectSoundFXI3DL2Reverb
{
	[PreserveSig]
	int SetAllParameters([In] ref DsFxI3Dl2Reverb param);

	[PreserveSig]
	int GetAllParameters(out DsFxI3Dl2Reverb param);

	[PreserveSig]
	int SetPreset([In] uint preset);

	[PreserveSig]
	int GetPreset(out uint preset);

	[PreserveSig]
	int SetQuality([In] int quality);

	[PreserveSig]
	int GetQuality(out int quality);
}
