using System.Runtime.InteropServices;
using System.Security;

namespace NAudio.Dmo.Effect;

[ComImport]
[SuppressUnmanagedCodeSecurity]
[Guid("880842e3-145f-43e6-a934-a71806e50547")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDirectSoundFXChorus
{
	[PreserveSig]
	int SetAllParameters([In] ref DsFxChorus param);

	[PreserveSig]
	int GetAllParameters(out DsFxChorus param);
}
