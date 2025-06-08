using System.Runtime.InteropServices;
using System.Security;

namespace NAudio.Dmo.Effect;

[ComImport]
[SuppressUnmanagedCodeSecurity]
[Guid("8ecf4326-455f-4d8b-bda9-8d5d3e9e3e0b")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDirectSoundFXDistortion
{
	[PreserveSig]
	int SetAllParameters([In] ref DsFxDistortion param);

	[PreserveSig]
	int GetAllParameters(out DsFxDistortion param);
}
