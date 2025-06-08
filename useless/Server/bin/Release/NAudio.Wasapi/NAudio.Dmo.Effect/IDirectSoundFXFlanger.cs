using System.Runtime.InteropServices;
using System.Security;

namespace NAudio.Dmo.Effect;

[ComImport]
[SuppressUnmanagedCodeSecurity]
[Guid("903e9878-2c92-4072-9b2c-ea68f5396783")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDirectSoundFXFlanger
{
	[PreserveSig]
	int SetAllParameters([In] ref DsFxFlanger param);

	[PreserveSig]
	int GetAllParameters(out DsFxFlanger param);
}
