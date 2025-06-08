using System.Runtime.InteropServices;

namespace NAudio.CoreAudioApi.Interfaces;

[ComImport]
[Guid("85401FD4-6DE4-4b9d-9869-2D6753A82F3C")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IAudioAutoGainControl
{
	[PreserveSig]
	int GetEnabled([MarshalAs(UnmanagedType.Bool)] out bool enabled);

	[PreserveSig]
	int SetEnabled([In][MarshalAs(UnmanagedType.Bool)] bool enabled);
}
