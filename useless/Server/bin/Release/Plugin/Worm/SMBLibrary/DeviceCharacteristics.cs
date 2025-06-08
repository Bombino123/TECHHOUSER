using System;
using System.Runtime.InteropServices;

namespace SMBLibrary;

[Flags]
[ComVisible(true)]
public enum DeviceCharacteristics : uint
{
	RemovableMedia = 1u,
	ReadOnlyDevice = 2u,
	FloppyDiskette = 4u,
	WriteOnceMedia = 8u,
	RemoteDevice = 0x10u,
	IsMounted = 0x20u,
	VirtualVolume = 0x40u
}
