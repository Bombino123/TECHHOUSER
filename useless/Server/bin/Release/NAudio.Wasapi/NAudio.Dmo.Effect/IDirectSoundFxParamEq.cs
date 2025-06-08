using System.Runtime.InteropServices;
using System.Security;

namespace NAudio.Dmo.Effect;

[ComImport]
[SuppressUnmanagedCodeSecurity]
[Guid("c03ca9fe-fe90-4204-8078-82334cd177da")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDirectSoundFxParamEq
{
	[PreserveSig]
	int SetAllParameters([In] ref DsFxParamEq param);

	[PreserveSig]
	int GetAllParameters(out DsFxParamEq param);
}
