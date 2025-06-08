using System.Runtime.InteropServices;
using System.Security;

namespace NAudio.Dmo.Effect;

[ComImport]
[SuppressUnmanagedCodeSecurity]
[Guid("8bd28edf-50db-4e92-a2bd-445488d1ed42")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDirectSoundFXEcho
{
	[PreserveSig]
	int SetAllParameters([In] ref DsFxEcho param);

	[PreserveSig]
	int GetAllParameters(out DsFxEcho param);
}
