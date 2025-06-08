using System.Runtime.InteropServices;

namespace NAudio.CoreAudioApi.Interfaces;

[ComImport]
[Guid("4509F757-2D46-4637-8E62-CE7DB944F57B")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IKsJackDescription
{
	int GetJackCount(out uint jacks);

	int GetJackDescription([In] uint jack, [MarshalAs(UnmanagedType.LPWStr)] out string description);
}
