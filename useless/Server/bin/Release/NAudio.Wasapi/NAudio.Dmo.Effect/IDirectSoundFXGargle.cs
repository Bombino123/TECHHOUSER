using System.Runtime.InteropServices;
using System.Security;

namespace NAudio.Dmo.Effect;

[ComImport]
[SuppressUnmanagedCodeSecurity]
[Guid("d616f352-d622-11ce-aac5-0020af0b99a3")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDirectSoundFXGargle
{
	[PreserveSig]
	int SetAllParameters([In] ref DsFxGargle param);

	[PreserveSig]
	int GetAllParameters(out DsFxGargle param);
}
