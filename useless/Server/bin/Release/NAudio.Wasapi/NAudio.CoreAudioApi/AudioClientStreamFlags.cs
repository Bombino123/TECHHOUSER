using System;

namespace NAudio.CoreAudioApi;

[Flags]
public enum AudioClientStreamFlags : uint
{
	None = 0u,
	CrossProcess = 0x10000u,
	Loopback = 0x20000u,
	EventCallback = 0x40000u,
	NoPersist = 0x80000u,
	RateAdjust = 0x100000u,
	SrcDefaultQuality = 0x8000000u,
	AutoConvertPcm = 0x80000000u
}
