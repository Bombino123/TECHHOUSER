using System;
using System.Runtime.InteropServices;

namespace NAudio.CoreAudioApi.Interfaces;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("726778CD-F60A-4eda-82DE-E47610CD78AA")]
public interface IAudioClient2 : IAudioClient
{
	void IsOffloadCapable(AudioStreamCategory category, out bool pbOffloadCapable);

	void SetClientProperties([In] IntPtr pProperties);

	void GetBufferSizeLimits(IntPtr pFormat, bool bEventDriven, out long phnsMinBufferDuration, out long phnsMaxBufferDuration);
}
