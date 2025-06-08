using System;
using System.Runtime.InteropServices;

namespace NAudio.CoreAudioApi.Interfaces;

[ComImport]
[Guid("DF45AEEA-B74A-4B6B-AFAD-2366B6AA012E")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IAudioMute
{
	[PreserveSig]
	int GetMute([MarshalAs(UnmanagedType.Bool)] out bool mute);

	[PreserveSig]
	int SetMute([In][MarshalAs(UnmanagedType.Bool)] bool mute, [In] ref Guid eventContext);
}
